using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeMonkeyPlayer;

namespace CodeMonkeyPlayerSetup
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }

    internal sealed class SetupForm : Form
    {
        private readonly ComboBox language = new ComboBox();
        private readonly CheckBox desktop = new CheckBox { Checked = true, AutoSize = true };
        private readonly CheckBox associations = new CheckBox { Checked = true, AutoSize = true };
        private readonly TextBox folder = new TextBox { Dock = DockStyle.Top };
        private readonly Label heading = NewLabel();
        private readonly Label intro = NewLabel();
        private readonly Label languageLabel = NewLabel();
        private readonly Label folderLabel = NewLabel();
        private readonly Label note = NewLabel();
        private readonly Label status = NewLabel();
        private readonly TextBox logLocation = new TextBox { ReadOnly = true, Dock = DockStyle.Top, Visible = false };
        private readonly Button install = new Button { AutoSize = true, MinimumSize = new Size(120, 36) };
        private readonly Button close = new Button { AutoSize = true, MinimumSize = new Size(100, 36) };
        private readonly ProgressBar progress = new ProgressBar { Dock = DockStyle.Top, Height = 12, Visible = false };
        private string code = "ko", statusKey = "Intro";
        private int? errorCode;
        private bool busy, completed;

        private static Label NewLabel() => new Label { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 6, 0, 8) };
        private string T(string key) => InstallerText.Get(code, key);

        public SetupForm()
        {
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(780, 720);
            MinimumSize = new Size(740, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(17, 20, 27);
            ForeColor = Color.FromArgb(235, 238, 245);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Player.ico"))
                Icon = new Icon(stream, new Size(64, 64));
            Disposed += (s, e) => Icon.Dispose();
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(32, 24, 32, 20), ColumnCount = 1, RowCount = 3 };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var brand = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 2, Margin = new Padding(0, 0, 0, 22) };
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var logo = new PictureBox { Size = new Size(60, 60), SizeMode = PictureBoxSizeMode.Zoom, Image = Icon.ToBitmap(), Margin = new Padding(0, 3, 16, 0) };
            Disposed += (s, e) => logo.Image.Dispose();
            brand.Controls.Add(logo, 0, 0);
            brand.SetRowSpan(logo, 2);
            brand.Controls.Add(new Label { Text = "CodeMonkey Player", AutoSize = true, Font = new Font(Font.FontFamily, 21F, FontStyle.Bold), Margin = Padding.Empty }, 1, 0);
            brand.Controls.Add(new Label { Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "   •   Windows x64", AutoSize = true, ForeColor = Color.FromArgb(151, 162, 184), Margin = new Padding(1, 5, 0, 0) }, 1, 1);
            shell.Controls.Add(brand, 0, 0);
            var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(24, 18, 24, 22), ColumnCount = 1, BackColor = Color.FromArgb(27, 32, 43) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            heading.Font = new Font(Font.FontFamily, 17F, FontStyle.Bold);
            intro.ForeColor = note.ForeColor = Color.FromArgb(164, 175, 196);
            intro.Margin = new Padding(0, 0, 0, 22);
            note.Margin = new Padding(0, 20, 0, 0);
            note.Font = new Font(Font.FontFamily, 9F);
            languageLabel.Font = folderLabel.Font = new Font(Font.FontFamily, 10F, FontStyle.Bold);
            language.DropDownStyle = ComboBoxStyle.DropDownList;
            language.Dock = DockStyle.Top;
            language.FlatStyle = FlatStyle.Flat;
            language.BackColor = Color.FromArgb(41, 48, 63);
            language.ForeColor = ForeColor;
            language.DrawMode = DrawMode.OwnerDrawFixed;
            language.ItemHeight = 30;
            language.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) != 0;
                using (var brush = new SolidBrush(selected ? Color.FromArgb(67, 57, 123) : language.BackColor))
                    e.Graphics.FillRectangle(brush, e.Bounds);
                var bounds = e.Bounds;
                bounds.Inflate(-10, 0);
                TextRenderer.DrawText(e.Graphics, language.Items[e.Index].ToString(), e.Font, bounds,
                    language.Enabled ? ForeColor : SystemColors.GrayText,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                    | (code == "ar" ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : TextFormatFlags.Left));
                e.DrawFocusRectangle();
            };
            language.Margin = new Padding(0, 0, 0, 16);
            foreach (var option in new[] { desktop, associations })
            {
                option.Margin = new Padding(0, 7, 0, 7);
                option.Padding = new Padding(0, 4, 0, 4);
                option.Cursor = Cursors.Hand;
            }
            folderLabel.Margin = new Padding(0, 20, 0, 8);
            foreach (var input in new[] { folder, logLocation })
            {
                input.BackColor = Color.FromArgb(41, 48, 63);
                input.ForeColor = ForeColor;
                input.BorderStyle = BorderStyle.FixedSingle;
                input.Margin = new Padding(0, 0, 0, 0);
            }
            language.Items.AddRange(UiText.Languages);
            language.SelectedIndex = 0;
            language.SelectedIndexChanged += (s, e) => ApplyLanguage(((UiText.LanguageOption)language.SelectedItem).Code);
            var programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
            if (string.IsNullOrEmpty(programFiles)) programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            folder.Text = Path.Combine(programFiles, "CodeMonkey Player");
            var footer = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 1, Margin = new Padding(0, 16, 0, 0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 14, 0, 0) };
            foreach (var button in new[] { install, close })
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Padding = new Padding(20, 8, 20, 8);
                button.MinimumSize = new Size(124, 44);
                button.Margin = new Padding(10, 0, 0, 0);
                button.Cursor = Cursors.Hand;
                button.BackColor = Color.FromArgb(41, 48, 63);
                button.ForeColor = ForeColor;
            }
            install.BackColor = Color.FromArgb(111, 92, 240);
            install.Font = new Font(Font, FontStyle.Bold);
            install.FlatAppearance.MouseOverBackColor = Color.FromArgb(132, 115, 255);
            close.FlatAppearance.MouseOverBackColor = Color.FromArgb(57, 66, 85);
            AcceptButton = install;
            CancelButton = close;
            buttons.Controls.Add(install);
            buttons.Controls.Add(close);
            foreach (Control control in new Control[] { heading, intro, languageLabel, language, desktop, associations, folderLabel, folder, note })
            {
                int row = layout.RowCount++;
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.Controls.Add(control, 0, row);
            }
            // Bound wrapping to the available width, including translations longer than English.
            layout.SizeChanged += (s, e) =>
            {
                int width = Math.Max(100, layout.ClientSize.Width - layout.Padding.Horizontal - 24);
                foreach (var label in new[] { heading, intro, languageLabel, folderLabel, note }) label.MaximumSize = new Size(width, 0);
                desktop.MaximumSize = associations.MaximumSize = new Size(width, 0);
            };
            foreach (Control control in new Control[] { progress, status, logLocation, buttons })
            {
                int row = footer.RowCount++;
                footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                footer.Controls.Add(control, 0, row);
            }
            footer.SizeChanged += (s, e) => status.MaximumSize = new Size(Math.Max(100, footer.ClientSize.Width), 0);
            viewport.Controls.Add(layout);
            shell.Controls.Add(viewport, 0, 1);
            shell.Controls.Add(footer, 0, 2);
            Controls.Add(shell);
            install.Click += async (s, e) => await InstallAsync();
            close.Click += (s, e) => Close();
            FormClosing += (s, e) => { if (busy) e.Cancel = true; };
            ApplyLanguage("ko");
        }

        private void ApplyLanguage(string selected)
        {
            code = UiText.Normalize(selected);
            Text = "CodeMonkey Player — " + T("Title");
            heading.Text = T("Title");
            intro.Text = T("Intro");
            languageLabel.Text = language.AccessibleName = T("Language");
            desktop.Text = T("Desktop");
            associations.Text = T("Associations");
            folderLabel.Text = folder.AccessibleName = T("Folder");
            note.Text = T("Note");
            install.Text = T("Install");
            close.Text = T(completed ? "Close" : "Cancel");
            status.Text = T(statusKey) + (errorCode.HasValue ? " (" + errorCode.Value + ")" : "");
            status.Visible = statusKey != "Intro";
            status.ForeColor = completed ? Color.FromArgb(113, 222, 170)
                : statusKey == "Failed" || statusKey == "InvalidPath" ? Color.FromArgb(255, 151, 151)
                : Color.FromArgb(188, 180, 255);
            AcceptButton = completed ? close : install;
            logLocation.AccessibleName = T("Log");
            RightToLeft = code == "ar" ? RightToLeft.Yes : RightToLeft.No;
            folder.RightToLeft = logLocation.RightToLeft = RightToLeft.No;
        }

        internal static string ValidateFolder(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(new[] { '"', '\r', '\n', '\0' }) >= 0)
                throw new ArgumentException();
            value = value.Trim();
            var full = Path.GetFullPath(value);
            if (value.Length < 4 || value[1] != ':' || value[2] != '\\' || full.Length < 4 || full.StartsWith(@"\\"))
                throw new ArgumentException();
            return full.TrimEnd('\\');
        }

        internal static string BuildArguments(string msi, string log, string destination, string languageCode, bool desktopShortcut, bool registerVideo)
        {
            return "/i \"" + msi + "\" /qn /norestart /L*v \"" + log + "\" INSTALLFOLDER=\"" + ValidateFolder(destination)
                + "\" APPLANGUAGE=" + UiText.Normalize(languageCode) + " DESKTOPSHORTCUT=" + (desktopShortcut ? "1" : "0")
                + " REGISTERVIDEO=" + (registerVideo ? "1" : "0");
        }

        private async Task InstallAsync()
        {
            try { folder.Text = ValidateFolder(folder.Text); }
            catch (Exception e) when (e is ArgumentException || e is NotSupportedException || e is PathTooLongException)
            { statusKey = "InvalidPath"; errorCode = null; ApplyLanguage(code); return; }
            busy = true;
            install.Enabled = close.Enabled = desktop.Enabled = associations.Enabled = folder.Enabled = language.Enabled = false;
            progress.Visible = true;
            progress.Style = ProgressBarStyle.Marquee;
            statusKey = "Installing";
            errorCode = null;
            ApplyLanguage(code);
            string temporary = null;
            try
            {
                // Retain the successful source package for Windows Installer repair.
                temporary = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.DoNotVerify), "CodeMonkeyPlayer", "SetupCache", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(temporary);
                string msi = Path.Combine(temporary, "Player.msi");
                string log = Path.Combine(Path.GetTempPath(), "CodeMonkeyPlayer-Setup-" + Guid.NewGuid().ToString("N") + ".log");
                using (var source = Assembly.GetExecutingAssembly().GetManifestResourceStream("Player.msi"))
                using (var target = File.Create(msi)) source.CopyTo(target);
                logLocation.Text = log;
                logLocation.Visible = true;
                var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "msiexec.exe"),
                    BuildArguments(msi, log, folder.Text, code, desktop.Checked, associations.Checked))
                    { UseShellExecute = true, Verb = "runas" };
                using (var process = Process.Start(start))
                {
                    await Task.Run(() => process.WaitForExit());
                    int exit = process.ExitCode;
                    completed = exit == 0 || exit == 3010;
                    statusKey = exit == 0 ? "Done" : exit == 3010 ? "Restart" : exit == 1602 ? "Cancelled" : "Failed";
                    if (!completed && exit != 1602) errorCode = exit;
                }
            }
            catch (Win32Exception e) { statusKey = e.NativeErrorCode == 1223 ? "Cancelled" : "Failed"; errorCode = e.NativeErrorCode; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is InvalidOperationException)
            { statusKey = "Failed"; errorCode = e.HResult; }
            finally
            {
                if (temporary != null && !completed)
                {
                    try { File.Delete(Path.Combine(temporary, "Player.msi")); Directory.Delete(temporary); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
                busy = false;
                close.Enabled = true;
                language.Enabled = true;
                install.Enabled = desktop.Enabled = associations.Enabled = folder.Enabled = !completed;
                progress.Visible = false;
                ApplyLanguage(code);
            }
        }
    }
}
