using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Lumen
{
    public record Verse(string Referencia, string Texto, int Livro = -1, int Cap = 0, int V1 = 0, int V2 = 0);

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
                {
                    var e = p.Value;
                    int I(string n) => e.TryGetProperty(n, out var x) ? x.GetInt32() : 0;
                    _days[p.Name] = new Verse(e.GetProperty("referencia").GetString()!, e.GetProperty("texto").GetString()!,
                        e.TryGetProperty("livro", out var l) ? l.GetInt32() : -1, I("cap"), I("v1"), I("v2"));
                }
        }

        public static Verse ForDate(DateTime d)
        {
            Load();
            return _days!.TryGetValue(d.ToString("MM-dd"), out var v) ? v : _days["01-01"];
        }
    }
}
