param([string]$Configuration='Debug')
$ErrorActionPreference='Stop'
if(Get-Process CodeMonkeyPlayer -ErrorAction SilentlyContinue){throw 'Close existing CodeMonkey Player windows before running this test.'}
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class PlaylistInspection {
  private delegate bool Callback(IntPtr h,IntPtr p);
  [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr h,Callback callback,IntPtr p);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetClassName(IntPtr h,StringBuilder b,int n);
  [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr h,int msg,IntPtr w,IntPtr l);
  [DllImport("user32.dll")] private static extern bool EnumWindows(Callback callback,IntPtr p);
  [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr h,out uint p);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetWindowText(IntPtr h,StringBuilder b,int n);
  public static IntPtr Window(int pid){IntPtr result=IntPtr.Zero;EnumWindows((h,p)=>{uint id;GetWindowThreadProcessId(h,out id);if(id==pid && Count(h)>=0)result=h;return true;},IntPtr.Zero);return result;}
  public static string Title(IntPtr h){var b=new StringBuilder(512);GetWindowText(h,b,512);return b.ToString();}
  public static int Count(IntPtr window){int count=-1;EnumChildWindows(window,(h,p)=>{var b=new StringBuilder(256);GetClassName(h,b,256);if(b.ToString().IndexOf("LISTBOX",StringComparison.OrdinalIgnoreCase)>=0)count=SendMessage(h,0x018B,IntPtr.Zero,IntPtr.Zero).ToInt32();return true;},IntPtr.Zero);return count;}
}
'@
$root=Split-Path -Parent $PSScriptRoot
$exe=Join-Path $root "CodeMonkeyPlayer/bin/$Configuration/CodeMonkeyPlayer.exe"
$runtime=Join-Path $PSScriptRoot ('settings-test-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runtime | Out-Null
$first=Join-Path $runtime 'first clip.wav';$second=Join-Path $runtime 'second clip.wav'
$writer=[IO.BinaryWriter]::new([IO.File]::Create($first))
try {$bytes=8000*2*30;$writer.Write([Text.Encoding]::ASCII.GetBytes('RIFF'));$writer.Write(36+$bytes);$writer.Write([Text.Encoding]::ASCII.GetBytes('WAVEfmt '));$writer.Write([int]16);$writer.Write([int16]1);$writer.Write([int16]1);$writer.Write([int]8000);$writer.Write([int]16000);$writer.Write([int16]2);$writer.Write([int16]16);$writer.Write([Text.Encoding]::ASCII.GetBytes('data'));$writer.Write($bytes);$writer.Write((New-Object byte[] $bytes))}finally{$writer.Dispose()}
Copy-Item -LiteralPath $first -Destination $second
$p=Start-Process -FilePath $exe -ArgumentList ('"'+$first+'"') -WindowStyle Hidden -PassThru
try {
    Start-Sleep -Seconds 3;$p.Refresh()
    if($p.HasExited){throw 'Primary process exited unexpectedly'}
    if([PlaylistInspection]::Count(([PlaylistInspection]::Window($p.Id))) -ne 1){throw ("First file was not loaded: handle="+([PlaylistInspection]::Window($p.Id))+" title="+([PlaylistInspection]::Title([PlaylistInspection]::Window($p.Id)))+" count="+[PlaylistInspection]::Count(([PlaylistInspection]::Window($p.Id))))}
    $q=Start-Process -FilePath $exe -ArgumentList ('"'+$second+'"') -WindowStyle Hidden -PassThru
    try {if(!$q.WaitForExit(7000)){ $q.Kill();throw 'Second launch did not hand off and exit' }}finally{$q.Dispose()}
    Start-Sleep -Milliseconds 500;$p.Refresh()
    if([PlaylistInspection]::Count(([PlaylistInspection]::Window($p.Id))) -ne 2){throw 'Existing playlist did not receive the second file'}
    if(!([PlaylistInspection]::Title([PlaylistInspection]::Window($p.Id))).Contains('second clip.wav')){throw 'Second file did not become the current track'}
    if(@(Get-Process CodeMonkeyPlayer).Count -ne 1){throw 'More than one player process remained'}
    Write-Output 'PASS: second OS launch exits, primary PID persists, playlist receives file and switches to the newly opened track.'
}finally{if(!$p.HasExited){$p.CloseMainWindow()|Out-Null;if(!$p.WaitForExit(5000)){$p.Kill()}};$p.Dispose()}
