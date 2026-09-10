using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using WMPLib;

namespace CodeMonkeyPlayer
{
    public partial class Form1 : Form
    {
        private readonly ListBox playlist = new ListBox();
        private readonly SeekTrackBar seek = new SeekTrackBar();
        private readonly SeekTrackBar volume = new SeekTrackBar { Compact = true };
        private readonly Label time = new Label();
        private readonly Label status = new Label();
        private readonly Button play = new Button();
        private readonly CheckBox mute = new CheckBox();
        private readonly CheckBox repeat = new CheckBox();
        private readonly Panel sidebar = new Panel();
        private readonly Panel transport = new Panel();
        private readonly Timer timer;
        private readonly Timer settingsSaveTimer;
        private readonly string settingsPath;
        private readonly string legacySettingsPath;
        private bool settingsReady;
        private Rectangle normalWindowBounds;
        private FormWindowState lastWindowState = FormWindowState.Normal;
        private bool changingWindowMode;
        private readonly Timer videoClickTimer;
        private readonly Timer dropRefreshTimer;
        private bool playingBeforeVideoClick;
        private int videoClickGeneration;
        private VideoFileDropTarget videoDropTarget;
        private ToolTip toolTips;
        private int currentIndex = -1;
        private int generation;
        private bool seeking, fullscreen;
        private bool frameStepBusy;
        private string frameRatePath;
        private double frameRate;
        private Rectangle restoredBounds;
        private FormWindowState restoredState;
        private static readonly string[] Extensions = { ".mp4", ".m4v", ".avi", ".wmv", ".mkv", ".mov", ".mpg", ".mpeg", ".webm", ".mp3", ".wav", ".wma", ".aac", ".flac", ".m4a" };

        public Form1() : this(PlayerPreferences.DefaultPath,
            Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "CodeMonkeyPlayer.ini"))
        {
        }

        internal Form1(string settingsPath, string legacySettingsPath)
        {
            this.settingsPath = settingsPath;
            this.legacySettingsPath = legacySettingsPath;
            InitializeComponent();
            using (var stream = typeof(Form1).Assembly.GetManifestResourceStream("CodeMonkeyPlayer.AppIcon.ico"))
            using (var sourceIcon = new Icon(stream))
            {
                var appIcon = (Icon)sourceIcon.Clone();
                Icon = appIcon;
                Disposed += (s, e) => appIcon.Dispose();
            }
            components = new System.ComponentModel.Container();
            toolTips = new ToolTip(components) { InitialDelay = 350, ReshowDelay = 100, ShowAlways = true };
            BuildInterface();
            settingsSaveTimer = new Timer(components) { Interval = 500 };
            settingsSaveTimer.Tick += (s, e) => SavePreferences();
            LocationChanged += (s, e) => RememberWindowPlacement();
            Resize += (s, e) => RememberWindowPlacement();
            volume.ValueChanged += (s, e) => ScheduleSettingsSave();
            mute.CheckedChanged += (s, e) => ScheduleSettingsSave();
            repeat.CheckedChanged += (s, e) => ScheduleSettingsSave();
            videoClickTimer = new Timer(components) { Interval = SystemInformation.DoubleClickTime };
            videoClickTimer.Tick += (s, e) =>
            {
                videoClickTimer.Stop();
            };
            dropRefreshTimer = new Timer(components) { Interval = 500 };
            dropRefreshTimer.Tick += (s, e) => { if (videoDropTarget != null) videoDropTarget.Refresh(); };
            timer = new Timer(components) { Interval = 50 };
            timer.Tick += (s, e) =>
            {
                UpdateTransportVisibility();
                UpdatePlayback();
            };
            Shown += (s, e) =>
            {
                axWindowsMediaPlayer1.uiMode = "none";
                axWindowsMediaPlayer1.stretchToFit = true;
                axWindowsMediaPlayer1.enableContextMenu = false;
                axWindowsMediaPlayer1.settings.autoStart = true;
                axWindowsMediaPlayer1.settings.volume = volume.Value;
                RestorePreferences();
                axWindowsMediaPlayer1.PlayStateChange += PlayerStateChanged;
                axWindowsMediaPlayer1.MediaError += (sender, args) => status.Text = "재생할 수 없습니다. 파일 손상 여부와 Windows 코덱 지원을 확인하세요.";
                axWindowsMediaPlayer1.KeyDownEvent += (sender, args) => HandleShortcut((Keys)args.nKeyCode | (ModifierKeys & Keys.Modifiers));
                axWindowsMediaPlayer1.ClickEvent += (sender, args) => VideoClicked(args.nButton);
                axWindowsMediaPlayer1.DoubleClickEvent += (sender, args) => VideoDoubleClicked(args.nButton);
                videoDropTarget = new VideoFileDropTarget(axWindowsMediaPlayer1, files =>
                {
                    videoClickTimer.Stop();
                    AddFiles(files);
                });
                components.Add(videoDropTarget);
                videoDropTarget.Refresh();
                dropRefreshTimer.Start();
                timer.Start();
                AddFiles(Environment.GetCommandLineArgs().Skip(1).ToArray());
            };
            FormClosing += (s, e) => { SavePreferences(); generation++; timer.Stop(); dropRefreshTimer.Stop(); videoClickTimer.Stop(); if (videoDropTarget != null) videoDropTarget.Dispose(); axWindowsMediaPlayer1.close(); };
        }

        private void RestorePreferences()
        {
            settingsReady = false;
            var settings = PlayerPreferences.Load(File.Exists(settingsPath) ? settingsPath : legacySettingsPath);
            volume.Value = settings.Volume;
            mute.Checked = settings.Muted;
            repeat.Checked = settings.Repeat;
            RestoreWindowPlacement(settings);
            axWindowsMediaPlayer1.settings.volume = volume.Value;
            axWindowsMediaPlayer1.settings.mute = mute.Checked;
            settingsReady = true;
            SavePreferences(); // Create the settings file on the first run as well.
        }

        private void ScheduleSettingsSave()
        {
            if (!settingsReady) return;
            settingsSaveTimer.Stop();
            settingsSaveTimer.Start();
        }

        private void SavePreferences()
        {
            settingsSaveTimer.Stop();
            if (!settingsReady) return;
            try
            {
                new PlayerPreferences
                {
                    Volume = volume.Value, Muted = mute.Checked, Repeat = repeat.Checked,
                    WindowBounds = fullscreen ? restoredBounds : normalWindowBounds,
                    WindowMaximized = (fullscreen ? restoredState : lastWindowState) == FormWindowState.Maximized
                }.Save(settingsPath);
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                status.Text = "설정을 저장하지 못했습니다. 사용자 설정 폴더의 쓰기 권한을 확인하세요.";
                toolTips.SetToolTip(status, settingsPath + "\n" + error.Message);
            }
        }

        private void RememberWindowPlacement()
        {
            if (!settingsReady || fullscreen || changingWindowMode) return;
            if (WindowState == FormWindowState.Normal) normalWindowBounds = Bounds;
            if (WindowState != FormWindowState.Minimized) lastWindowState = WindowState;
            ScheduleSettingsSave();
        }

        private void RestoreWindowPlacement(PlayerPreferences settings)
        {
            WindowState = FormWindowState.Normal;
            if (settings.WindowBounds.HasValue)
            {
                Rectangle saved = settings.WindowBounds.Value;
                Rectangle area = Screen.FromRectangle(saved).WorkingArea;
                int width = Math.Min(area.Width, Math.Max(MinimumSize.Width, saved.Width));
                int height = Math.Min(area.Height, Math.Max(MinimumSize.Height, saved.Height));
                // A removed monitor or changed display scaling must not strand the title bar.
                MinimumSize = new Size(Math.Min(MinimumSize.Width, area.Width), Math.Min(MinimumSize.Height, area.Height));
                StartPosition = FormStartPosition.Manual;
                Bounds = new Rectangle(Math.Max(area.Left, Math.Min(saved.X, area.Right - width)),
                    Math.Max(area.Top, Math.Min(saved.Y, area.Bottom - height)), width, height);
            }
            normalWindowBounds = Bounds;
            lastWindowState = settings.WindowMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
            WindowState = lastWindowState;
        }

        private void BuildInterface()
        {
            Text = "CodeMonkey Player";
            MinimumSize = new Size(900, 500);
            ClientSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("맑은 고딕", 9F);
            BackColor = Color.FromArgb(22, 24, 30);
            ForeColor = Color.WhiteSmoke;
            KeyPreview = true;
            bar1.Visible = false;
            axWindowsMediaPlayer1.Dock = DockStyle.Fill;
            sidebar.Dock = DockStyle.Right;
            sidebar.Width = 240;
            sidebar.Padding = new Padding(12);
            sidebar.BackColor = Color.FromArgb(31, 34, 42);
            var heading = new Label { Text = "재생 목록", Dock = DockStyle.Top, Height = 36, Font = new Font(Font, FontStyle.Bold) };
            playlist.Dock = DockStyle.Fill;
            playlist.BackColor = sidebar.BackColor;
            playlist.ForeColor = ForeColor;
            playlist.BorderStyle = BorderStyle.None;
            playlist.IntegralHeight = false;
            playlist.HorizontalScrollbar = true;
            playlist.AllowDrop = true;
            playlist.DragEnter += Form1_DragEnter;
            playlist.DragDrop += Form1_DragDrop;
            playlist.DoubleClick += (s, e) => PlayIndex(playlist.SelectedIndex);
            playlist.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) { RemoveSelected(); e.Handled = true; } };
            var listActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
            listActions.Controls.Add(MakeButton("추가", (s, e) => OpenFiles(), 65));
            listActions.Controls.Add(MakeButton("삭제", (s, e) => RemoveSelected(), 65));
            listActions.Controls.Add(MakeButton("비우기", (s, e) => ClearPlaylist(), 65));
            sidebar.Controls.Add(playlist);
            sidebar.Controls.Add(heading);
            sidebar.Controls.Add(listActions);
            transport.Dock = DockStyle.Bottom;
            transport.Height = 94;
            transport.Padding = new Padding(12, 0, 12, 6);
            transport.BackColor = Color.FromArgb(20, 20, 20);
            transport.ForeColor = Color.WhiteSmoke;
            seek.Dock = DockStyle.Top;
            seek.Maximum = 10000;
            seek.BackColor = transport.BackColor;
            seek.Height = 46;
            seek.AccessibleName = "재생 위치";
            seek.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                seeking = true;
                CommitSeek();
            };
            seek.MouseUp += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                CommitSeek();
                seeking = false;
            };
            seek.MouseCaptureChanged += (s, e) => { if (!seek.Capture) seeking = false; };
            seek.KeyUp += (s, e) =>
            {
                // Only Home/End modify this slider. Other shortcut key releases must
                // never seek back to a stale slider value after a frame step.
                if (e.KeyCode == Keys.Home || e.KeyCode == Keys.End) CommitSeek();
            };
            time.Text = "00:00 / 00:00";
            time.AutoSize = false;
            time.Size = new Size(135, 38);
            time.TextAlign = ContentAlignment.MiddleLeft;
            time.Margin = new Padding(10, 0, 0, 0);

            var row = new Panel { Dock = DockStyle.Fill };
            var actions = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 460, WrapContents = false };
            var secondary = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 200, WrapContents = false, FlowDirection = FlowDirection.RightToLeft };
            row.Controls.Add(actions);
            row.Controls.Add(secondary);

            actions.Controls.Add(MakeButton("이전", (s, e) => MoveTrack(-1), 55));
            SetIcon(play, "재생", "Space");
            StyleButton(play, 80);
            play.Click += (s, e) => TogglePlayback();
            actions.Controls.Add(play);

            actions.Controls.Add(MakeButton("다음", (s, e) => MoveTrack(1), 55));
            StyleToggle(mute, "음소거", "M");
            mute.CheckedChanged += (s, e) =>
            {
                axWindowsMediaPlayer1.settings.mute = mute.Checked;
                SetIcon(mute, mute.Checked ? "음소거 해제" : "음소거", "M");
            };
            actions.Controls.Add(mute);
            volume.Maximum = 100;
            volume.Value = 70;
            volume.Size = new Size(80, 38);
            volume.Margin = new Padding(0);
            volume.BackColor = transport.BackColor;

            volume.AccessibleName = "음량";
            volume.ValueChanged += (s, e) => axWindowsMediaPlayer1.settings.volume = volume.Value;
            actions.Controls.Add(volume);
            actions.Controls.Add(time);
            StyleToggle(repeat, "목록 반복 켜기", "");
            repeat.CheckedChanged += (s, e) =>
                SetIcon(repeat, repeat.Checked ? "목록 반복 끄기" : "목록 반복 켜기", "");

            secondary.Controls.Add(MakeButton("전체 화면", (s, e) => ToggleFullscreen(), 85));
            secondary.Controls.Add(MakeButton("도움말", (s, e) => MessageBox.Show(this,
                "Ctrl+O: 파일 열기\nSpace: 재생 / 일시 정지\n, / .: 이전 / 다음 프레임 (지원하는 영상)\n← / →: 5초 이동\n↑ / ↓: 음량 조절\nM: 음소거\nPageUp / PageDown: 이전 / 다음 파일\nF11: 전체 화면\nEsc: 전체 화면 종료\nDelete: 선택한 목록 항목 삭제\n\n파일을 끌어다 놓거나 재생 목록을 두 번 클릭하세요.", "사용 방법"), 65));
            secondary.Controls.Add(repeat);
            secondary.Controls.Add(MakeButton("파일 열기", (s, e) => OpenFiles(), 38));
            secondary.Controls.Add(MakeButton("정지", (s, e) => { generation++; axWindowsMediaPlayer1.Ctlcontrols.stop(); status.Text = "정지"; }, 38));
            status.Dock = DockStyle.Bottom;
            status.Height = 64;
            status.Padding = new Padding(0, 8, 0, 0);
            status.ForeColor = Color.FromArgb(170, 174, 182);
            sidebar.Controls.Add(status);
            status.BringToFront();
            status.AutoEllipsis = true;
            status.Text = "파일을 열거나 영상 / 재생 목록으로 끌어다 놓으세요.";

            transport.Controls.Add(row);

            transport.Controls.Add(seek);
            Controls.Add(sidebar);
            Controls.Add(transport);
            axWindowsMediaPlayer1.BringToFront();
        }

        private Button MakeButton(string text, EventHandler click, int width)
        {
            var button = new Button();
            string shortcut = text == "파일 열기" ? "Ctrl+O" : text == "이전" ? "PageUp" : text == "다음" ? "PageDown" : text == "전체 화면" ? "F11" : text == "삭제" ? "Delete" : "";
            SetIcon(button, text, shortcut);
            StyleButton(button, width);
            button.Click += click;
            return button;
        }

        private void StyleButton(Button button, int width)
        {
            button.Width = 38;
            button.Height = 38;
            button.Paint += PaintIcon;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(20, 20, 20);
            button.Margin = new Padding(1, 0, 1, 0);
            button.ForeColor = Color.WhiteSmoke;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(75, 75, 75);
        }

        private void SetIcon(ButtonBase button, string name, string shortcut)
        {
            if (button.AccessibleName == name) return;
            button.Text = "";
            button.AccessibleName = name;
            toolTips.SetToolTip(button, name + (shortcut.Length > 0 ? " (" + shortcut + ")" : ""));
            button.Invalidate();
        }

        private void StyleToggle(CheckBox button, string name, string shortcut)
        {
            button.Appearance = Appearance.Button;
            button.AutoSize = false;
            button.Size = new Size(38, 38);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(20, 20, 20);
            button.Margin = new Padding(1, 0, 1, 0);
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(75, 75, 75);
            button.FlatAppearance.CheckedBackColor = Color.FromArgb(100, 35, 40);
            button.Paint += PaintIcon;
            SetIcon(button, name, shortcut);
        }

        private void PaintIcon(object sender, PaintEventArgs e)
        {
            var button = (ButtonBase)sender;
            var g = e.Graphics;
            var saved = g.Save();
            float size = Math.Min(button.ClientSize.Width, button.ClientSize.Height) * 0.62F;
            g.TranslateTransform((button.ClientSize.Width - size) / 2, (button.ClientSize.Height - size) / 2);
            g.ScaleTransform(size / 24, size / 24);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(button.Enabled ? Color.WhiteSmoke : Color.Gray, 1.8F))
            using (var brush = new SolidBrush(pen.Color))
            {
                pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                switch (button.AccessibleName)
                {
                    case "재생":
                        g.FillPolygon(brush, new[] { new Point(7, 3), new Point(21, 12), new Point(7, 21) }); break;
                    case "일시 정지":
                        g.FillRectangle(brush, 5, 4, 5, 16); g.FillRectangle(brush, 14, 4, 5, 16); break;
                    case "정지": g.FillRectangle(brush, 5, 5, 14, 14); break;
                    case "이전":
                    case "다음":
                        if (button.AccessibleName == "이전") { g.TranslateTransform(24, 0); g.ScaleTransform(-1, 1); }
                        g.FillPolygon(brush, new[] { new Point(4, 4), new Point(17, 12), new Point(4, 20) });
                        g.DrawLine(pen, 20, 4, 20, 20); break;
                    case "파일 열기":
                        g.DrawLines(pen, new[] { new Point(2, 19), new Point(2, 5), new Point(9, 5), new Point(12, 8), new Point(21, 8), new Point(21, 11) });
                        g.DrawPolygon(pen, new[] { new Point(2, 20), new Point(6, 11), new Point(23, 11), new Point(19, 20) }); break;
                    case "추가": g.DrawLine(pen, 12, 4, 12, 20); g.DrawLine(pen, 4, 12, 20, 12); break;
                    case "삭제": g.DrawLine(pen, 5, 12, 19, 12); break;
                    case "비우기":
                        g.DrawLine(pen, 4, 6, 20, 6); g.DrawLine(pen, 9, 3, 15, 3);
                        g.DrawLines(pen, new[] { new Point(6, 9), new Point(7, 21), new Point(17, 21), new Point(18, 9) });
                        g.DrawLine(pen, 10, 10, 10, 17); g.DrawLine(pen, 14, 10, 14, 17); break;
                    case "음소거":
                    case "음소거 해제":
                        g.DrawPolygon(pen, new[] { new Point(2, 9), new Point(6, 9), new Point(12, 4), new Point(12, 20), new Point(6, 15), new Point(2, 15) });
                        if (button.AccessibleName == "음소거 해제")
                        { g.DrawLine(pen, 16, 9, 22, 15); g.DrawLine(pen, 22, 9, 16, 15); }
                        else { g.DrawArc(pen, 11, 7, 8, 10, -65, 130); g.DrawArc(pen, 10, 3, 13, 18, -60, 120); }
                        break;
                    case "목록 반복 켜기":
                    case "목록 반복 끄기":
                        g.DrawLines(pen, new[] { new Point(3, 10), new Point(3, 6), new Point(20, 6), new Point(16, 2) });
                        g.DrawLine(pen, 20, 6, 16, 10);
                        g.DrawLines(pen, new[] { new Point(21, 14), new Point(21, 18), new Point(4, 18), new Point(8, 22) });
                        g.DrawLine(pen, 4, 18, 8, 14); break;
                    case "전체 화면":
                        g.DrawLines(pen, new[] { new Point(3, 9), new Point(3, 3), new Point(9, 3) });
                        g.DrawLines(pen, new[] { new Point(15, 3), new Point(21, 3), new Point(21, 9) });
                        g.DrawLines(pen, new[] { new Point(3, 15), new Point(3, 21), new Point(9, 21) });
                        g.DrawLines(pen, new[] { new Point(15, 21), new Point(21, 21), new Point(21, 15) }); break;
                    case "도움말":
                        g.DrawEllipse(pen, 2, 2, 20, 20);
                        g.DrawArc(pen, 8, 6, 8, 7, 180, 240); g.DrawLine(pen, 12, 13, 12, 14);
                        g.FillEllipse(brush, 11, 17, 2, 2); break;
                }
            }
            g.Restore(saved);
        }

        private void OpenFiles()
        {
            using (var dialog = new OpenFileDialog { Multiselect = true, Title = "재생할 파일 선택",
                Filter = "미디어 파일|" + string.Join(";", Extensions.Select(x => "*" + x)) + "|모든 파일|*.*" })
                if (dialog.ShowDialog(this) == DialogResult.OK) AddFiles(dialog.FileNames);
        }

        private void AddFiles(string[] files)
        {
            int first = -1, rejected = 0;
            foreach (string file in files)
            {
                if (!File.Exists(file) || !Extensions.Contains(Path.GetExtension(file).ToLowerInvariant())) { rejected++; continue; }
                string path = Path.GetFullPath(file);
                int index = -1;
                for (int i = 0; i < playlist.Items.Count; i++)
                    if (string.Equals(((MediaItem)playlist.Items[i]).Path, path, StringComparison.OrdinalIgnoreCase)) { index = i; break; }
                if (index < 0) index = playlist.Items.Add(new MediaItem(path));
                if (first < 0) first = index;
            }
            if (first >= 0) PlayIndex(first);
            if (rejected > 0) status.Text = rejected + "개 파일을 추가하지 못했습니다. 지원하는 미디어 파일인지 확인하세요.";
        }

        private void PlayIndex(int index)
        {
            if (index < 0 || index >= playlist.Items.Count) return;
            var item = (MediaItem)playlist.Items[index];
            if (!File.Exists(item.Path)) { status.Text = "파일을 찾을 수 없습니다: " + item.Path; return; }
            generation++;
            videoClickTimer.Stop();
            currentIndex = index;
            playlist.SelectedIndex = index;
            seek.Value = 0;
            Text = item + " — CodeMonkey Player";
            status.Text = "여는 중: " + item;
            axWindowsMediaPlayer1.URL = item.Path;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void VideoClicked(int button)
        {
            if (button != 1 || currentIndex < 0) return;
            if (!videoClickTimer.Enabled)
            {
                playingBeforeVideoClick = axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying;
                videoClickGeneration = generation;
            }
            TogglePlayback();
            videoClickGeneration = generation;
            videoClickTimer.Stop();
            videoClickTimer.Start();
        }

        private void VideoDoubleClicked(int button)
        {
            if (button != 1) return;
            // The first click responds immediately; a double click restores its prior state.
            if (videoClickTimer.Enabled && videoClickGeneration == generation && currentIndex >= 0)
            {
                if (playingBeforeVideoClick) axWindowsMediaPlayer1.Ctlcontrols.play();
                else axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            videoClickTimer.Stop();
            ToggleFullscreen();
        }

        private void TogglePlayback()
        {
            generation++;
            if (currentIndex < 0) { if (playlist.Items.Count > 0) PlayIndex(0); else OpenFiles(); return; }
            if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying) axWindowsMediaPlayer1.Ctlcontrols.pause();
            else axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void MoveTrack(int direction)
        {
            int count = playlist.Items.Count;
            if (count == 0) return;
            int index = currentIndex < 0 ? 0 : currentIndex + direction;
            if (repeat.Checked) index = (index + count) % count;
            PlayIndex(index);
        }

        private void RemoveSelected()
        {
            int index = playlist.SelectedIndex;
            if (index < 0) return;
            generation++;
            bool active = index == currentIndex;
            if (active) { currentIndex = -1; axWindowsMediaPlayer1.close(); }
            playlist.Items.RemoveAt(index);
            if (active && playlist.Items.Count > 0) PlayIndex(Math.Min(index, playlist.Items.Count - 1));
            else if (!active && index < currentIndex) currentIndex--;
            if (playlist.Items.Count == 0) ResetDisplay();
        }

        private void ClearPlaylist()
        {
            generation++;
            currentIndex = -1;
            axWindowsMediaPlayer1.close();
            playlist.Items.Clear();
            ResetDisplay();
        }

        private void ResetDisplay()
        {
            Text = "CodeMonkey Player";
            status.Text = "파일을 열거나 영상 / 재생 목록으로 끌어다 놓으세요.";
            seek.Value = 0;
            time.Text = "00:00 / 00:00";
        }

        private double Duration { get { return axWindowsMediaPlayer1.currentMedia == null ? 0 : axWindowsMediaPlayer1.currentMedia.duration; } }

        private void UpdatePlayback()
        {
            double duration = Duration;
            seek.Duration = duration;
            double position = axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
            seek.Enabled = duration > 0 && axWindowsMediaPlayer1.Ctlcontrols.get_isAvailable("currentPosition");
            if (!seeking) seek.Value = duration > 0 ? (int)Math.Max(0, Math.Min(10000, position / duration * 10000)) : 0;
            time.Text = FormatTime(seeking ? seek.Value / 10000.0 * duration : position) + " / " + FormatTime(duration);
            SetIcon(play, axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying ? "일시 정지" : "재생", "Space");
        }

        private static string FormatTime(double seconds)
        {
            var value = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return value.TotalHours >= 1 ? ((int)value.TotalHours).ToString("00") + value.ToString(@"\:mm\:ss") : value.ToString(@"mm\:ss");
        }

        private void CommitSeek()
        {
            generation++;
            if (seek.Enabled && Duration > 0) axWindowsMediaPlayer1.Ctlcontrols.currentPosition = seek.Value / 10000.0 * Duration;
        }

        private async void StepFrame(int direction)
        {
            if (frameStepBusy || currentIndex < 0 || axWindowsMediaPlayer1.currentMedia == null) return;
            var controls = axWindowsMediaPlayer1.Ctlcontrols as IWMPControls2;
            if (controls == null || !controls.get_isAvailable("step"))
            {
                status.Text = "이 파일 또는 코덱은 프레임 이동을 지원하지 않습니다.";
                return;
            }
            videoClickTimer.Stop();
            int request = ++generation;
            frameStepBusy = true;
            try
            {
                double expectedPrevious = -1;
                double fps = 0;
                // WMP commands are asynchronous. Wait for pause before issuing a step.
                controls.pause();
                for (int i = 0; i < 50 && axWindowsMediaPlayer1.playState != WMPPlayState.wmppsPaused; i++)
                {
                    await System.Threading.Tasks.Task.Delay(20);
                    if (IsDisposed || request != generation) return;
                }
                if (axWindowsMediaPlayer1.playState != WMPPlayState.wmppsPaused) return;
                if (direction < 0)
                {
                    fps = ReadFrameRate(axWindowsMediaPlayer1.URL);
                    if (fps <= 0)
                    {
                        status.Text = "영상의 FPS를 확인할 수 없어 이전 프레임 이동을 사용할 수 없습니다.";
                        return;
                    }
                    double origin = controls.currentPosition;
                    if (origin < 1.0 / fps) return;
                    // Do not call step(-1): some decoders jump to the previous keyframe.
                    // Some decoders use the first forward step only to paint the
                    // sought frame; others advance. Start at the desired timestamp.
                    double target = Math.Max(0, origin - 1.0 / fps);
                    expectedPrevious = target;
                    controls.currentPosition = target;
                    int stable = 0;
                    for (int i = 0; i < 50 && stable < 3; i++)
                    {
                        await System.Threading.Tasks.Task.Delay(20);
                        if (IsDisposed || request != generation) return;
                        stable = Math.Abs(controls.currentPosition - target) < 0.5 / fps ? stable + 1 : 0;
                    }
                    if (stable < 3)
                    {
                        status.Text = "이 코덱은 정밀 탐색을 지원하지 않아 프레임 이동을 완료하지 못했습니다.";
                        return;
                    }
                }
                if (IsDisposed || request != generation) return;
                double beforeStep = controls.currentPosition;
                controls.step(1);
                // Avoid overlapping step requests while the decoder updates its frame.
                await System.Threading.Tasks.Task.Delay(80);
                if (IsDisposed || request != generation) return;
                if (direction > 0)
                {
                    // A first step after seeking may only refresh the current frame.
                    for (int i = 0; i < 2 && controls.currentPosition <= beforeStep + 0.0001; i++)
                    {
                        controls.step(1);
                        await System.Threading.Tasks.Task.Delay(80);
                        if (IsDisposed || request != generation) return;
                    }
                }
                else if (controls.currentPosition > expectedPrevious + 0.5 / fps)
                {
                    // This decoder advanced while painting: compensate exactly once.
                    controls.currentPosition = Math.Max(0, expectedPrevious - 1.0 / fps);
                    await System.Threading.Tasks.Task.Delay(80);
                    if (IsDisposed || request != generation) return;
                    controls.step(1);
                    await System.Threading.Tasks.Task.Delay(80);
                    if (IsDisposed || request != generation) return;
                }
                UpdatePlayback();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                if (!IsDisposed) status.Text = "프레임을 이동하지 못했습니다. 파일 또는 코덱의 지원 여부를 확인하세요.";
            }
            finally { frameStepBusy = false; }
        }

        private double ReadFrameRate(string path)
        {
            if (frameRatePath == path) return frameRate;
            frameRatePath = path;
            frameRate = 0;
            object shell = null, folder = null, item = null;
            try
            {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                folder = ((dynamic)shell).NameSpace(Path.GetDirectoryName(path));
                if (folder == null) return 0;
                item = ((dynamic)folder).ParseName(Path.GetFileName(path));
                if (item == null) return 0;
                object value = ((dynamic)item).ExtendedProperty("System.Video.FrameRate");
                // Windows exposes frames per 1000 seconds, not frames per second.
                double fps = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) / 1000.0;
                if (fps > 0 && fps <= 1000) frameRate = fps;
            }
            catch (System.Runtime.InteropServices.COMException) { }
            catch (FormatException) { }
            catch (InvalidCastException) { }
            finally
            {
                foreach (object value in new[] { item, folder, shell })
                    if (value != null && System.Runtime.InteropServices.Marshal.IsComObject(value))
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(value);
            }
            return frameRate;
        }
        private void PlayerStateChanged(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            var state = (WMPPlayState)e.newState;
            if (state == WMPPlayState.wmppsPlaying) status.Text = "재생 중: " + Path.GetFileName(axWindowsMediaPlayer1.URL);
            else if (state == WMPPlayState.wmppsPaused) status.Text = "일시 정지";
            else if (state == WMPPlayState.wmppsBuffering) status.Text = "버퍼링 중…";
            else if (state == WMPPlayState.wmppsMediaEnded)
            {
                status.Text = "재생 완료";
                if (frameStepBusy) return;
                int endedGeneration = generation;
                // Defer URL changes until the COM state callback returns.
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed && generation == endedGeneration) MoveTrack(1);
                }));
            }
        }

        private void ToggleFullscreen()
        {
            changingWindowMode = true;
            if (!fullscreen)
            {
                restoredState = WindowState;
                restoredBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                Bounds = Screen.FromControl(this).Bounds;
                sidebar.Visible = false;
                transport.Visible = false;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                Bounds = restoredBounds;
                WindowState = restoredState;
                sidebar.Visible = true;
                transport.Visible = true;
            }
            fullscreen = !fullscreen;
            changingWindowMode = false;
            RememberWindowPlacement();
            ScheduleSettingsSave();
        }

        private void UpdateTransportVisibility()
        {
            if (!fullscreen) return;

            // Poll the cursor because the embedded ActiveX window consumes mouse events.
            // Keep the entire toolbar available after revealing it, including while dragging.
            if (seeking || Control.MouseButtons != MouseButtons.None) return;
            Point cursor = PointToClient(Cursor.Position);
            int revealHeight = transport.Visible ? transport.Height : 32;
            bool atBottom = ClientRectangle.Contains(cursor)
                && cursor.Y >= ClientSize.Height - revealHeight;
            transport.Visible = ContainsFocus && atBottom;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return HandleShortcut(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }

        private bool HandleShortcut(Keys key)
        {
            switch (key)
            {
                case Keys.Control | Keys.O: OpenFiles(); return true;
                case Keys.Space: TogglePlayback(); return true;
                case Keys.Oemcomma: StepFrame(-1); return true;
                case Keys.OemPeriod: StepFrame(1); return true;
                case Keys.F11: ToggleFullscreen(); return true;
                case Keys.Escape: if (fullscreen) ToggleFullscreen(); return true;
                case Keys.M: mute.Checked = !mute.Checked; return true;
                case Keys.PageDown: MoveTrack(1); return true;
                case Keys.PageUp: MoveTrack(-1); return true;
                case Keys.Up: volume.Value = Math.Min(100, volume.Value + 5); return true;
                case Keys.Down: volume.Value = Math.Max(0, volume.Value - 5); return true;
                case Keys.Left:
                case Keys.Right:
                    generation++;
                    if (Duration > 0 && axWindowsMediaPlayer1.Ctlcontrols.get_isAvailable("currentPosition"))
                        axWindowsMediaPlayer1.Ctlcontrols.currentPosition = Math.Max(0, Math.Min(Duration, axWindowsMediaPlayer1.Ctlcontrols.currentPosition + (key == Keys.Right ? 5 : -5)));
                    return true;
                default: return false;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null) AddFiles(files);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private sealed class SeekTrackBar : Control
        {
            private int position;
            private bool hovering;
            private int pointerX;
            private double duration;
            public bool Compact { get; set; }
            public event EventHandler ValueChanged;
            public int Maximum { get; set; } = 10000;
            public int Value
            {
                get { return position; }
                set
                {
                    int next = Math.Max(0, Math.Min(Maximum, value));
                    if (position == next) return;
                    position = next;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
            public double Duration
            {
                get { return duration; }
                set { if (duration != value) { duration = value; Invalidate(); } }
            }
            private float DpiScale { get { return DeviceDpi / 96F; } }
            private float Inset { get { return 8 * DpiScale; } }
            private float TrackWidth { get { return Math.Max(1, ClientSize.Width - 2 * Inset); } }
            private double FractionAt(int x) { return Math.Max(0, Math.Min(1, (x - Inset) / TrackWidth)); }

            public SeekTrackBar()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                    | ControlStyles.Selectable, true);
                TabStop = true;
                Cursor = Cursors.Hand;
                AccessibleRole = AccessibleRole.Slider;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                bool active = Enabled && (hovering || Capture || Focused);
                float thickness = (active ? 5 : 3) * DpiScale;
                float centerY = Compact ? Height / 2F : Height - 10 * DpiScale;
                float playedWidth = TrackWidth * Value / Math.Max(1, Maximum);
                using (var track = new SolidBrush(Color.FromArgb(90, 92, 98)))
                using (var preview = new SolidBrush(Color.FromArgb(155, 157, 162)))
                using (var red = new SolidBrush(Enabled ? (Compact ? Color.WhiteSmoke : Color.FromArgb(255, 40, 55)) : Color.FromArgb(120, 65, 70)))
                {
                    g.FillRectangle(track, Inset, centerY - thickness / 2, TrackWidth, thickness);
                    if (Enabled && (hovering || Capture))
                        g.FillRectangle(preview, Inset, centerY - thickness / 2, (float)FractionAt(pointerX) * TrackWidth, thickness);
                    g.FillRectangle(red, Inset, centerY - thickness / 2, playedWidth, thickness);
                    if (active)
                    {
                        float radius = 6 * DpiScale;
                        g.FillEllipse(red, Inset + playedWidth - radius, centerY - radius, radius * 2, radius * 2);
                    }
                }
                if (!Compact && Enabled && duration > 0 && (hovering || Capture))
                {
                    string label = FormatTime(FractionAt(pointerX) * duration);
                    Size textSize = TextRenderer.MeasureText(label, Font);
                    int width = textSize.Width + (int)(12 * DpiScale);
                    int height = textSize.Height + (int)(4 * DpiScale);
                    int left = Math.Max(0, Math.Min(Width - width, pointerX - width / 2));
                    var bubble = new Rectangle(left, Math.Max(0, (int)(centerY - 11 * DpiScale) - height), width, height);
                    using (var background = new SolidBrush(Color.FromArgb(18, 18, 20)))
                        g.FillRectangle(background, bubble);
                    TextRenderer.DrawText(g, label, Font, bubble, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                hovering = true;
                pointerX = PointToClient(MousePosition).X;
                Invalidate();
                base.OnMouseEnter(e);
            }
            protected override void OnMouseLeave(EventArgs e)
            {
                hovering = false;
                Invalidate();
                base.OnMouseLeave(e);
            }
            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (Enabled && e.Button == MouseButtons.Left)
                {
                    Focus();
                    Capture = true;
                    pointerX = e.X;
                    Value = (int)Math.Round(FractionAt(e.X) * Maximum);
                    Invalidate();
                }
                base.OnMouseDown(e);
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                pointerX = e.X;
                if (Enabled && Capture) Value = (int)Math.Round(FractionAt(e.X) * Maximum);
                Invalidate();
                base.OnMouseMove(e);
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (Enabled && e.Button == MouseButtons.Left && Capture)
                {
                    pointerX = e.X;
                    Value = (int)Math.Round(FractionAt(e.X) * Maximum);
                }
                base.OnMouseUp(e);
                if (e.Button == MouseButtons.Left) Capture = false;
            }
            protected override void OnMouseCaptureChanged(EventArgs e)
            {
                Invalidate();
                base.OnMouseCaptureChanged(e);
            }
            protected override void OnEnabledChanged(EventArgs e)
            {
                if (!Enabled) Capture = false;
                Invalidate();
                base.OnEnabledChanged(e);
            }
            protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
            protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Home) { Value = 0; e.Handled = true; }
                if (e.KeyCode == Keys.End) { Value = Maximum; e.Handled = true; }
                base.OnKeyDown(e);
            }
        }
        private sealed class MediaItem
        {
            public string Path { get; private set; }
            public MediaItem(string path) { Path = path; }
            public override string ToString() { return System.IO.Path.GetFileName(Path); }
        }
    }
}









