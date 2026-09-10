param([Parameter(Mandatory=$true)][string]$MediaPath,[string]$Configuration='Debug')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class MpvInputTest {
    private delegate bool Callback(IntPtr h,IntPtr p);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr h,Callback cb,IntPtr p);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetClassName(IntPtr h,StringBuilder b,int n);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h,int m,IntPtr w,IntPtr l);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr h,ref Point p);
    [StructLayout(LayoutKind.Sequential)] private struct Point {public int X,Y;}
    public static int HitTest(IntPtr h){var p=new Point{X=20,Y=20};ClientToScreen(h,ref p);return SendMessage(h,0x0084,IntPtr.Zero,new IntPtr((p.X & 65535)|((p.Y & 65535)<<16))).ToInt32();}
    public static IntPtr Window(IntPtr parent){IntPtr result=IntPtr.Zero;EnumChildWindows(parent,(h,p)=>{var b=new StringBuilder(256);GetClassName(h,b,256);if(b.ToString().IndexOf("mpv",StringComparison.OrdinalIgnoreCase)>=0)result=h;return true;},IntPtr.Zero);return result;}
}
'@
$root=Split-Path -Parent $PSScriptRoot
$bin=Join-Path $root "CodeMonkeyPlayer/bin/$Configuration"
$a=[Reflection.Assembly]::LoadFrom((Join-Path $bin 'CodeMonkeyPlayer.exe'))
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$type=$a.GetType('CodeMonkeyPlayer.Form1')
$runtime=Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
$f=$type.GetConstructor($flags,$null,[type[]]@([string],[string]),$null).Invoke(@((Join-Path $runtime 'prefs.ini').PSObject.BaseObject,(Join-Path $runtime 'legacy.ini').PSObject.BaseObject))
function Field($name){$type.GetField($name,$flags).GetValue($f)}
function Pump($milliseconds){$watch=[Diagnostics.Stopwatch]::StartNew();while($watch.ElapsedMilliseconds -lt $milliseconds){[Windows.Forms.Application]::DoEvents();Start-Sleep -Milliseconds 10}}
function InvokePrivate($name,[object[]]$arguments){$type.GetMethod($name,$flags).Invoke($f,$arguments)}
try {
    $f.StartPosition='Manual';$f.Location=[Drawing.Point]::new(-30000,-30000);$f.ShowInTaskbar=$false
    $f.Show()
    $video=Field 'video'
    $video.Muted=$true
    InvokePrivate 'AddFiles' (,([string[]]@($MediaPath))) | Out-Null
    Pump 4000
    Write-Output "State=$($video.State) Duration=$($video.Duration) Position=$($video.Position) Format=$($video.VideoFormat) FPS=$($video.FrameRate)"
    Write-Output ('Status='+(Field 'status').Text)
    if($video.Duration -le 0 -or $video.Position -le 0) {throw 'mpv did not play the media'}
    $video.Pause();Pump 250
    $video.Seek(10);Pump 1000
    $before=$video.Position
    $video.Step(1);Pump 500
    $forward=$video.Position
    $video.Step(-1);Pump 1000
    $back=$video.Position
    Write-Output "Frame step: before=$before forward=$forward back=$back"
    if($forward -le $before -or $back -ge $forward -or $video.State -ne 'Paused'){throw 'Frame stepping failed'}
    $window=[MpvInputTest]::Window($video.Handle)
    if($window -eq [IntPtr]::Zero){throw 'Embedded mpv video window was not created'}
    if([MpvInputTest]::HitTest($window) -ne 1){throw 'Native video window does not accept client mouse input'}
    [MpvInputTest]::PostMessage($window,0x0201,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($window,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 350
    if($video.State -ne 'Playing'){throw 'Native video click did not reach playback controls'}
    $video.Pause();Pump 250
    # Real host-panel messages must work too (letterboxing/loading/render-window transitions).
    Pump 600
    [MpvInputTest]::PostMessage($video.Handle,0x0201,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($video.Handle,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 200
    if($video.State -ne 'Playing'){throw 'Host video panel click did not resume playback'}
    Pump 600
    [MpvInputTest]::PostMessage($video.Handle,0x0201,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($video.Handle,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 200
    if($video.State -ne 'Paused'){throw 'Host video panel click did not pause playback'}
    [MpvInputTest]::PostMessage($video.Handle,0x0203,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($video.Handle,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 200
    if(!(Field 'fullscreen') -or $video.State -ne 'Playing'){throw 'Host double click did not preserve initial state and enter fullscreen'}
    InvokePrivate 'ToggleFullscreen' @() | Out-Null
    $video.Pause();Pump 600
    $window=[MpvInputTest]::Window($video.Handle)
    [MpvInputTest]::PostMessage($window,0x0201,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($window,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 100
    [MpvInputTest]::PostMessage($window,0x0201,[IntPtr]1,[IntPtr]0x00140014) | Out-Null
    [MpvInputTest]::PostMessage($window,0x0202,[IntPtr]0,[IntPtr]0x00140014) | Out-Null
    Pump 250
    if(!(Field 'fullscreen') -or $video.State -ne 'Paused'){throw ("Native double click: fullscreen="+(Field 'fullscreen')+" state="+$video.State)}
    InvokePrivate 'ToggleFullscreen' @() | Out-Null
    $type.GetMethod('ReceiveFiles',$flags).Invoke($f,(,[string[]]@($MediaPath))) | Out-Null
    if((Field 'playlist').Items.Count -ne 1){throw 'Repeated external open added duplicate'}
    Pump 1500
    if($video.State -ne 'Playing'){throw 'External open did not start playback'}
    $second=Join-Path $runtime ('second'+[IO.Path]::GetExtension($MediaPath))
    Copy-Item -LiteralPath $MediaPath -Destination $second
    $type.GetMethod('ReceiveFiles',$flags).Invoke($f,(,[string[]]@($second))) | Out-Null
    Pump 1500
    if((Field 'playlist').Items.Count -ne 2 -or $video.MediaPath -ne $second -or $video.State -ne 'Playing'){throw 'Opening another file did not add it and switch playback'}
    $watch=[Diagnostics.Stopwatch]::StartNew();$f.Close()
    if($watch.ElapsedMilliseconds -gt 1500){throw 'UI close blocked on engine shutdown'}
    Write-Output 'PASS: mpv playback, forward/backward stepping, native video click, external duplicate suppression, external-open playback, responsive close.'
} finally {$f.Dispose()}
