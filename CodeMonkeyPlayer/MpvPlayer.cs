using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CodeMonkeyPlayer
{
    internal enum PlaybackState { Stopped, Playing, Paused, Buffering, Ended }

    internal sealed class MpvPlayer : Panel
    {
        private IntPtr context;
        private readonly ConcurrentQueue<string[]> commands = new ConcurrentQueue<string[]>();
        private volatile bool shuttingDown;
        private bool started, loaded, paused;
        private int volume = 70;
        private bool muted;
        public string MediaPath { get; private set; } = "";
        public double Duration { get; private set; }
        public double Position { get; private set; }
        public double FrameRate { get; private set; }
        public string VideoFormat { get; private set; } = "";
        public long DecodedFrames { get; private set; }
        public PlaybackState State { get; private set; }
        public event EventHandler StateChanged;
        public event EventHandler MediaError;
        public event Action<bool> VideoClick;
        public event Action<Keys> VideoKey;
        public int Volume { get => volume; set { volume = Math.Max(0, Math.Min(100, value)); Command("set", "volume", volume.ToString(CultureInfo.InvariantCulture)); } }
        public bool Muted { get => muted; set { muted = value; Command("set", "mute", value ? "yes" : "no"); } }

        public MpvPlayer()
        {
            BackColor = Color.Black;
            AllowDrop = true;
            // The host panel can receive clicks outside/replacing the native VO
            // window. Child-window mouse messages do not bubble to this panel.
            MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) VideoClick?.Invoke(false); };
            MouseDoubleClick += (s, e) => { if (e.Button == MouseButtons.Left) VideoClick?.Invoke(true); };
        }
        public void Initialize()
        {
            if (started) return;
            started = true;
            var window = Handle;
            new Thread(() => Run(window)) { IsBackground = true, Name = "mpv playback" }.Start();
        }
        public void Load(string path)
        {
            MediaPath = path;
            Duration = Position = FrameRate = 0;
            DecodedFrames = 0;
            VideoFormat = "";
            ChangeState(PlaybackState.Buffering);
            Command("loadfile", path, "replace");
            Command("set", "pause", "no");
        }
        public void Play() { Command("set", "pause", "no"); }
        public void Pause() { Command("set", "pause", "yes"); }
        public void Stop() { Command("stop"); MediaPath = ""; Duration = Position = 0; ChangeState(PlaybackState.Stopped); }
        public void Seek(double seconds) { Command("seek", Math.Max(0, seconds).ToString("R", CultureInfo.InvariantCulture), "absolute+exact"); }
        public void Step(int direction) { Command(direction < 0 ? "frame-back-step" : "frame-step"); }
        private void Command(params string[] args) { if (!shuttingDown) commands.Enqueue(args); }
        public void Shutdown() { shuttingDown = true; }
        protected override void Dispose(bool disposing) { Shutdown(); base.Dispose(disposing); }
        private void Post(Action action)
        {
            if (shuttingDown || IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(new Action(() => { if (!shuttingDown && !IsDisposed) action(); })); }
            catch (InvalidOperationException) { }
        }
        private void ChangeState(PlaybackState value) { State = value; StateChanged?.Invoke(this, EventArgs.Empty); }
        private void Run(IntPtr window)
        {
            try
            {
                context = mpv_create();
                if (context == IntPtr.Zero) throw new InvalidOperationException("mpv_create");
                Option("wid", window.ToInt64().ToString(CultureInfo.InvariantCulture));
                Option("config", "no"); Option("load-scripts", "no"); Option("ytdl", "no");
                Option("input-default-bindings", "no"); Option("input-vo-keyboard", "yes");
                Option("input-cursor", "yes"); Option("input-cursor-passthrough", "no");
                Option("window-dragging", "no"); Option("input-builtin-dragging", "no");
                Option("input-doubleclick-time", SystemInformation.DoubleClickTime.ToString(CultureInfo.InvariantCulture));
                Option("osc", "no"); Option("osd-level", "0"); Option("idle", "yes");
                Option("keep-open", "no"); Option("hwdec", "auto-safe");
                Option("vo", "gpu"); Option("gpu-api", "d3d11");
                Check(mpv_initialize(context));
                foreach (string name in new[] { "time-pos", "duration", "estimated-vf-fps", "pause", "video-format", "decoder-frame-drop-count", "estimated-frame-number" })
                    Check(mpv_observe_property(context, 0, name, 1));
                Execute(new[] { "keybind", "MBTN_LEFT", "script-message cmp-click" });
                Execute(new[] { "keybind", "MBTN_LEFT_DBL", "script-message cmp-double" });
                foreach (string key in new[] { "SPACE", "LEFT", "RIGHT", "UP", "DOWN", "m", "PGUP", "PGDWN", "F11", "ESC", ",", ".", "Ctrl+o" })
                    Execute(new[] { "keybind", key, "script-message cmp-key " + key });
                while (!shuttingDown)
                {
                    int sent = 0;
                    while (sent++ < 64 && commands.TryDequeue(out var command)) Execute(command);
                    var evt = Marshal.PtrToStructure<MpvEvent>(mpv_wait_event(context, 0.02));
                    if (evt.Id == 22 && evt.Data != IntPtr.Zero)
                    {
                        var property = Marshal.PtrToStructure<MpvProperty>(evt.Data);
                        string name = Utf8(property.Name);
                        string value = property.Format == 1 && property.Data != IntPtr.Zero ? Utf8(Marshal.ReadIntPtr(property.Data)) : "";
                        Post(() => PropertyChanged(name, value));
                    }
                    else if (evt.Id == 8) Post(() => { loaded = true; ChangeState(paused ? PlaybackState.Paused : PlaybackState.Playing); });
                    else if (evt.Id == 7 && evt.Data != IntPtr.Zero)
                    {
                        int reason = Marshal.ReadInt32(evt.Data);
                        Post(() => { loaded = false; if (reason == 0) ChangeState(PlaybackState.Ended); else if (reason == 4) { ChangeState(PlaybackState.Stopped); MediaError?.Invoke(this, EventArgs.Empty); } });
                    }
                    else if (evt.Id == 16 && evt.Data != IntPtr.Zero)
                    {
                        var message = Marshal.PtrToStructure<MpvMessage>(evt.Data);
                        if (message.Count < 1) continue;
                        string kind = Utf8(Marshal.ReadIntPtr(message.Args));
                        if (kind == "cmp-click" || kind == "cmp-double") Post(() => VideoClick?.Invoke(kind == "cmp-double"));
                        else if (kind == "cmp-key" && message.Count > 1)
                        {
                            string key = Utf8(Marshal.ReadIntPtr(message.Args, IntPtr.Size));
                            Post(() => VideoKey?.Invoke(MapKey(key)));
                        }
                    }
                }
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException || e is BadImageFormatException || e is InvalidOperationException)
            { Post(() => MediaError?.Invoke(this, EventArgs.Empty)); }
            finally { if (context != IntPtr.Zero) mpv_terminate_destroy(context); }
        }
        private static Keys MapKey(string key)
        {
            switch (key) { case "SPACE": return Keys.Space; case "LEFT": return Keys.Left; case "RIGHT": return Keys.Right; case "UP": return Keys.Up; case "DOWN": return Keys.Down; case "m": return Keys.M; case "PGUP": return Keys.PageUp; case "PGDWN": return Keys.PageDown; case "F11": return Keys.F11; case "ESC": return Keys.Escape; case ",": return Keys.Oemcomma; case ".": return Keys.OemPeriod; case "Ctrl+o": return Keys.Control | Keys.O; default: return Keys.None; }
        }
        private void PropertyChanged(string name, string value)
        {
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number);
            if (name == "time-pos") Position = number;
            else if (name == "duration") Duration = number;
            else if (name == "estimated-vf-fps") FrameRate = number;
            else if (name == "video-format") VideoFormat = value;
            else if (name == "estimated-frame-number") DecodedFrames = (long)number;
            else if (name == "pause") { paused = value == "yes"; if (loaded) ChangeState(paused ? PlaybackState.Paused : PlaybackState.Playing); }
        }
        private void Option(string name, string value) { Check(mpv_set_option_string(context, name, value)); }
        private static void Check(int code) { if (code < 0) throw new InvalidOperationException("mpv error " + code); }
        private void Execute(string[] args)
        {
            var pointers = new IntPtr[args.Length + 1];
            IntPtr array = Marshal.AllocHGlobal(pointers.Length * IntPtr.Size);
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(args[i] + "\0");
                    pointers[i] = Marshal.AllocHGlobal(bytes.Length); Marshal.Copy(bytes, 0, pointers[i], bytes.Length);
                }
                Marshal.Copy(pointers, 0, array, pointers.Length);
                if (mpv_command(context, array) < 0) Post(() => MediaError?.Invoke(this, EventArgs.Empty));
            }
            finally { foreach (var pointer in pointers) if (pointer != IntPtr.Zero) Marshal.FreeHGlobal(pointer); Marshal.FreeHGlobal(array); }
        }
        private static string Utf8(IntPtr p)
        {
            if (p == IntPtr.Zero) return "";
            int length = 0; while (Marshal.ReadByte(p, length) != 0) length++;
            byte[] bytes = new byte[length]; Marshal.Copy(p, bytes, 0, length); return Encoding.UTF8.GetString(bytes);
        }
        [StructLayout(LayoutKind.Sequential)] private struct MpvEvent { public int Id, Error; public ulong UserData; public IntPtr Data; }
        [StructLayout(LayoutKind.Sequential)] private struct MpvProperty { public IntPtr Name; public int Format; public IntPtr Data; }
        [StructLayout(LayoutKind.Sequential)] private struct MpvMessage { public int Count; public IntPtr Args; }
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr mpv_create();
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern int mpv_initialize(IntPtr c);
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern int mpv_set_option_string(IntPtr c, string name, string value);
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern int mpv_command(IntPtr c, IntPtr args);
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern int mpv_observe_property(IntPtr c, ulong id, string name, int format);
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr mpv_wait_event(IntPtr c, double timeout);
        [DllImport("libmpv-2.dll", CallingConvention = CallingConvention.Cdecl)] private static extern void mpv_terminate_destroy(IntPtr c);
    }
}
