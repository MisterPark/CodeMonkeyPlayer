using System;
using System.IO;
using System.Text;

namespace CodeMonkeyPlayer
{
    internal sealed class PlayerPreferences
    {
        public int Volume { get; set; } = 70;
        public bool Muted { get; set; }
        public bool Repeat { get; set; }

        public static PlayerPreferences Load(string path)
        {
            var settings = new PlayerPreferences();
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
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return settings;
        }

        public void Save(string path)
        {
            // Replace atomically so interrupted writes do not truncate existing settings.
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                string contents = "[CodeMonkeyPlayer]\r\n"
                    + "Volume=" + Math.Max(0, Math.Min(100, Volume)) + "\r\n"
                    + "Muted=" + Muted.ToString().ToLowerInvariant() + "\r\n"
                    + "Repeat=" + Repeat.ToString().ToLowerInvariant() + "\r\n";
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
