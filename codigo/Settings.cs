using System;
using System.IO;
using System.Text.Json;

namespace Lumen
{
    public class Settings
    {
        public int? Left { get; set; }
        public int? Top { get; set; }
        public bool AlwaysOnTop { get; set; } = false;
        public bool FirstRunDone { get; set; } = false;
        public bool LockScreen { get; set; } = true;
        public string TextSize { get; set; } = "Medio"; // Pequeno, Medio, Grande
        public float SizeFactor => TextSize switch { "Pequeno" => 0.85f, "Grande" => 1.2f, _ => 1f };

        public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lumen");
        private static string FilePath => Path.Combine(Folder, "config.json");

        public static Settings Load()
        {
            try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings(); }
            catch { }
            return new Settings();
        }

        public void Save()
        {
            try { Directory.CreateDirectory(Folder); File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
            catch { }
        }
    }
}
