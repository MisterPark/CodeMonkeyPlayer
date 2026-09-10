using System;
using System.IO;
using System.Text;
using System.Drawing;

namespace CodeMonkeyPlayer
{
    internal sealed class PlayerPreferences
    {
        public static string DefaultPath
        {
            get
            {
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.DoNotVerify);
                if (string.IsNullOrEmpty(directory)) directory = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                if (string.IsNullOrEmpty(directory) || !Path.IsPathRooted(directory))
                    throw new InvalidOperationException("사용자 설정 폴더를 찾을 수 없습니다.");
                return Path.Combine(directory, "CodeMonkeyPlayer", "CodeMonkeyPlayer.ini");
            }
        }
        public int Volume { get; set; } = 70;
        public bool Muted { get; set; }
        public bool Repeat { get; set; }
        public Rectangle? WindowBounds { get; set; }
        public bool WindowMaximized { get; set; }

        public static PlayerPreferences Load(string path)
        {
            var settings = new PlayerPreferences();
            int? x = null, y = null, width = null, height = null;
            try
            {
                foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    int separator = line.IndexOf('=');
                    if (separator < 0) continue;
                    string key = line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();
                    int number;
                    bool flag;
                    if (key.Equals("Volume", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out number))
                        settings.Volume = Math.Max(0, Math.Min(100, number));
                    else if (key.Equals("Muted", StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out flag))
                        settings.Muted = flag;
                    else if (key.Equals("Repeat", StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out flag))
                        settings.Repeat = flag;
                    else if (key.Equals("WindowMaximized", StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out flag))
                        settings.WindowMaximized = flag;
                    else if (int.TryParse(value, out number))
                    {
                        switch (key.ToLowerInvariant())
                        {
                            case "windowx": x = number; break;
                            case "windowy": y = number; break;
                            case "windowwidth": width = number; break;
                            case "windowheight": height = number; break;
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            if (x.HasValue && y.HasValue && width > 0 && width <= 32768 && height > 0 && height <= 32768
                && Math.Abs((long)x.Value) <= 1000000 && Math.Abs((long)y.Value) <= 1000000)
                settings.WindowBounds = new Rectangle(x.Value, y.Value, width.Value, height.Value);
            return settings;
        }

        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            // Replace atomically so interrupted writes do not truncate existing settings.
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                string contents = "[CodeMonkeyPlayer]\r\n"
                    + "Volume=" + Math.Max(0, Math.Min(100, Volume)) + "\r\n"
                    + "Muted=" + Muted.ToString().ToLowerInvariant() + "\r\n"
                    + "Repeat=" + Repeat.ToString().ToLowerInvariant() + "\r\n";
                contents += "WindowMaximized=" + WindowMaximized.ToString().ToLowerInvariant() + "\r\n";
                if (WindowBounds.HasValue)
                {
                    Rectangle bounds = WindowBounds.Value;
                    contents += "WindowX=" + bounds.X + "\r\nWindowY=" + bounds.Y
                        + "\r\nWindowWidth=" + bounds.Width + "\r\nWindowHeight=" + bounds.Height + "\r\n";
                }
                File.WriteAllText(temporary, contents, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
