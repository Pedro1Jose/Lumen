using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lumen
{
    // A Bíblia completa (Matos Soares), usada para mostrar o capítulo inteiro do versículo do dia.
    // O texto não fica no repositório: o build baixa o biblia.json de github.com/Dancrf/biblia-db e o embute no .exe.
    public static class Bible
    {
        public record Book(string Nome, List<List<(int Num, string Texto)>> Capitulos);
        private static List<Book>? _books;

        private static readonly string[] Nomes = (
            "Gênesis|Êxodo|Levítico|Números|Deuteronômio|Josué|Juízes|Rute|1 Samuel|2 Samuel|1 Reis|2 Reis|1 Crônicas|2 Crônicas|" +
            "Esdras|Neemias|Tobias|Judite|Ester|1 Macabeus|2 Macabeus|Jó|Salmo|Provérbios|Eclesiastes|Cântico dos Cânticos|Sabedoria|" +
            "Eclesiástico|Isaías|Jeremias|Lamentações|Baruc|Ezequiel|Daniel|Oseias|Joel|Amós|Abdias|Jonas|Miqueias|Naum|Habacuc|" +
            "Sofonias|Ageu|Zacarias|Malaquias|Mateus|Marcos|Lucas|João|Atos|Romanos|1 Coríntios|2 Coríntios|Gálatas|Efésios|" +
            "Filipenses|Colossenses|1 Tessalonicenses|2 Tessalonicenses|1 Timóteo|2 Timóteo|Tito|Filêmon|Hebreus|Tiago|1 Pedro|" +
            "2 Pedro|1 João|2 João|3 João|Judas|Apocalipse").Split('|');

        private static readonly Regex Espacos = new(@"\s{2,}");

        public static List<Book> Books
        {
            get
            {
                if (_books != null) return _books;
                using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("biblia.json")!;
                using var doc = JsonDocument.Parse(s);
                var list = new List<Book>();
                int i = 0;
                foreach (var b in doc.RootElement.EnumerateArray())
                {
                    var caps = new List<List<(int, string)>>();
                    foreach (var c in b.GetProperty("capitulos").EnumerateArray())
                    {
                        var vs = new List<(int, string)>();
                        foreach (var v in c.GetProperty("versiculos").EnumerateArray())
                        {
                            string t = (v.GetProperty("texto").GetString() ?? "").Replace("(ver nota)", "");
                            vs.Add((v.GetProperty("numero").GetInt32(), Espacos.Replace(t, " ").Trim()));
                        }
                        caps.Add(vs);
                    }
                    list.Add(new Book(i < Nomes.Length ? Nomes[i] : b.GetProperty("livro").GetString() ?? "", caps));
                    i++;
                }
                return _books = list;
            }
        }
    }
}
