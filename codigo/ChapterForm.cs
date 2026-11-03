using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Lumen
{
    // Janela que mostra o capítulo inteiro, com o versículo do dia destacado.
    public class ChapterForm : Form
    {
        private static ChapterForm? _open;
        private readonly RichTextBox _box;
        private readonly Label _title;
        private readonly Button _prev, _next;
        private readonly Verse _verse;
        private int _cap;

        public static void ShowFor(Verse v)
        {
            if (v.Livro < 0) return;
            if (_open != null && !_open.IsDisposed) _open.Close();
            _open = new ChapterForm(v);
            _open.Show();
            _open.Activate();
        }

        private ChapterForm(Verse v)
        {
            _verse = v; _cap = v.Cap;
            Text = "Lumen";
            using (var ico = Assembly.GetExecutingAssembly().GetManifestResourceStream("lumen.ico")!) Icon = new Icon(ico);
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(620, 720);
            MinimumSize = new Size(420, 400);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(0x16, 0x1E, 0x33);

            var top = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 10, 16, 6), BackColor = BackColor };
            _title = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(0xFF, 0xCC, 0x5C), Font = new Font("Segoe UI Semibold", 15f), TextAlign = ContentAlignment.MiddleLeft };
            _prev = NavButton("‹"); _next = NavButton("›");
            _prev.Click += (_, _) => LoadChapter(_cap - 1);
            _next.Click += (_, _) => LoadChapter(_cap + 1);
            top.Controls.Add(_title); top.Controls.Add(_prev); top.Controls.Add(_next);
            _prev.Dock = DockStyle.Right; _next.Dock = DockStyle.Right;

            _box = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
                BackColor = BackColor, ForeColor = Color.FromArgb(0xEE, 0xE9, 0xDD),
                Font = new Font("Georgia", 13f), DetectUrls = false, ScrollBars = RichTextBoxScrollBars.Vertical
            };
            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 6, 12, 16), BackColor = BackColor };
            pad.Controls.Add(_box);
            Controls.Add(pad); Controls.Add(top);
            KeyPreview = true;
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); else if (e.KeyCode == Keys.Left) LoadChapter(_cap - 1); else if (e.KeyCode == Keys.Right) LoadChapter(_cap + 1); };
            Shown += (_, _) => LoadChapter(_cap);
        }

        private Button NavButton(string t) => new()
        {
            Text = t, Width = 40, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(0xFF, 0xCC, 0x5C),
            Font = new Font("Segoe UI", 16f), FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(0x24, 0x31, 0x52) }, TabStop = false
        };

        private void LoadChapter(int cap)
        {
            var book = Bible.Books[_verse.Livro];
            if (cap < 1 || cap > book.Capitulos.Count) return;
            _cap = cap;
            _title.Text = $"{book.Nome} {cap}";
            _prev.Enabled = cap > 1; _next.Enabled = cap < book.Capitulos.Count;

            _box.SuspendLayout();
            _box.Clear();
            int highlightStart = -1;
            var normal = _box.Font;
            using var bold = new Font(normal, FontStyle.Bold);
            using var num = new Font("Segoe UI", 8.5f);
            foreach (var (n, texto) in book.Capitulos[cap - 1])
            {
                bool today = cap == _verse.Cap && n >= _verse.V1 && n <= _verse.V2;
                if (today && highlightStart < 0) highlightStart = _box.TextLength;
                _box.SelectionStart = _box.TextLength;
                _box.SelectionFont = num;
                _box.SelectionColor = Color.FromArgb(0xE8, 0xB2, 0x4A);
                _box.SelectionCharOffset = 6;
                _box.AppendText(n + " ");
                _box.SelectionCharOffset = 0;
                _box.SelectionFont = today ? bold : normal;
                _box.SelectionColor = today ? Color.FromArgb(0xFF, 0xDA, 0x85) : Color.FromArgb(0xEE, 0xE9, 0xDD);
                _box.AppendText(texto + "\n\n");
            }
            _box.ResumeLayout();
            _box.SelectionStart = Math.Max(0, highlightStart);
            _box.SelectionLength = 0;
            _box.ScrollToCaret();
            if (highlightStart < 0) { _box.SelectionStart = 0; _box.ScrollToCaret(); }
        }
    }
}
