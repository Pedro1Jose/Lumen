using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Reflection;

namespace Lumen
{
    // Desenha o cartão do versículo numa imagem com transparência.
    // Usado pelo widget e, na etapa 3, também pela imagem da tela de bloqueio.
    public static class CardRenderer
    {
        private static Image? _logo;
        public static Image Logo => _logo ??= Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("lumen.png")!);

        // Estilo minimalista (o padrão do Lumen): sem cartão, só a lamparina, uma linha dourada,
        // o versículo em branco e a referência em dourado, com uma sombra suave para ler em qualquer papel de parede.
        public static Bitmap Render(Verse v, DateTime date, float scale, int widthDip = 380)
        {
            int W = (int)(widthDip * scale);
            float pad = 14 * scale;
            using var verseFont = new Font("Georgia", 15f * scale, GraphicsUnit.Pixel);
            using var refFont = new Font("Segoe UI", 12.5f * scale, FontStyle.Italic, GraphicsUnit.Pixel);
            string refStr = "— " + v.Referencia;
            float textW = W - 2 * pad;

            SizeF verseSize, refSize;
            using (var tmp = new Bitmap(1, 1))
            using (var g0 = Graphics.FromImage(tmp))
            {
                g0.TextRenderingHint = TextRenderingHint.AntiAlias;
                verseSize = g0.MeasureString(v.Texto, verseFont, (int)textW);
                refSize = g0.MeasureString(refStr, refFont);
            }

            float logo = 18 * scale, headerGap = 10 * scale, gap = 8 * scale;
            int H = (int)Math.Ceiling(pad + logo + headerGap + verseSize.Height + gap + refSize.Height + pad);

            var bmp = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Fundo quase invisível: não aparece, mas deixa a área toda clicável para arrastar.
            g.Clear(Color.FromArgb(1, 0, 0, 0));

            float x = pad, y = pad;
            g.DrawImage(Logo, x, y, logo, logo);
            using (var line = new Pen(Color.FromArgb(170, 0xE8, 0xB2, 0x4A), 1.5f * scale))
                g.DrawLine(line, x + logo + 10 * scale, y + logo / 2, x + logo + 10 * scale + 60 * scale, y + logo / 2);
            y += logo + headerGap;

            var rect = new RectangleF(x, y, textW, verseSize.Height + 4);
            DrawSoft(g, v.Texto, verseFont, rect, Color.FromArgb(245, 0xFF, 0xF7, 0xEA), scale);
            y += verseSize.Height + gap;
            DrawSoft(g, refStr, refFont, new RectangleF(x, y, textW, refSize.Height + 4), Color.FromArgb(0xF0, 0xBE, 0x5A), scale);
            return bmp;
        }

        private static void DrawSoft(Graphics g, string text, Font font, RectangleF r, Color color, float scale)
        {
            using (var sh = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                foreach (var (dx, dy) in new[] { (1f, 1f), (2f, 2f), (-1f, 1f), (1f, -1f), (0f, 2.5f) })
                    g.DrawString(text, font, sh, new RectangleF(r.X + dx * scale, r.Y + dy * scale, r.Width, r.Height));
            using var b = new SolidBrush(color);
            g.DrawString(text, font, b, r);
        }

        public static GraphicsPath RoundRect(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
