using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Lumen
{
    public record Verse(string Referencia, string Texto);

    public static class Verses
    {
        private static Dictionary<string, Verse>? _days;
        public static string Translation { get; private set; } = "";

        private static void Load()
        {
            if (_days != null) return;
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("versiculos.json")!;
            using var doc = JsonDocument.Parse(s);
            Translation = doc.RootElement.GetProperty("traducao").GetString() ?? "";
            _days = new Dictionary<string, Verse>();
            foreach (var p in doc.RootElement.GetProperty("dias").EnumerateObject())
                _days[p.Name] = new Verse(p.Value.GetProperty("referencia").GetString()!, p.Value.GetProperty("texto").GetString()!);
        }

        public static Verse ForDate(DateTime d)
        {
            Load();
            return _days!.TryGetValue(d.ToString("MM-dd"), out var v) ? v : _days["01-01"];
        }
    }
}
