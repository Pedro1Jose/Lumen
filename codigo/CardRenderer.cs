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

        public static Bitmap Render(Verse v, DateTime date, float scale, int widthDip = 380)
        {
            int W = (int)(widthDip * scale);
            float pad = 20 * scale, radius = 16 * scale;
            using var dateFont = new Font("Segoe UI Semibold", 9f * scale, GraphicsUnit.Point);
            using var verseFont = new Font("Georgia", 13f * scale, GraphicsUnit.Point);
            using var refFont = new Font("Segoe UI", 10f * scale, FontStyle.Italic, GraphicsUnit.Point);

            var pt = new CultureInfo("pt-BR");
            string dateStr = date.ToString("dddd, d 'de' MMMM", pt).ToUpper(pt);
            float textW = W - 2 * pad;

            SizeF verseSize, refSize;
            using (var tmp = new Bitmap(1, 1))
            using (var g0 = Graphics.FromImage(tmp))
            {
                g0.TextRenderingHint = TextRenderingHint.AntiAlias;
                verseSize = g0.MeasureString(v.Texto, verseFont, (int)textW);
                refSize = g0.MeasureString(v.Referencia, refFont);
            }

            float headerH = 22 * scale, gap = 10 * scale;
            int H = (int)Math.Ceiling(pad + headerH + gap + verseSize.Height + gap + refSize.Height + pad);
            int shadow = (int)(10 * scale);

            var bmp = new Bitmap(W + 2 * shadow, H + 2 * shadow, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);

            // Sombra suave
            for (int i = shadow; i > 0; i -= Math.Max(1, shadow / 5))
            {
                using var sp = RoundRect(new RectangleF(shadow - i * 0.6f, shadow - i * 0.3f, W + i * 1.2f, H + i * 1.2f), radius + i);
                using var sb = new SolidBrush(Color.FromArgb(10, 0, 0, 0));
                g.FillPath(sb, sp);
            }

            var card = new RectangleF(shadow, shadow, W, H);
            using (var path = RoundRect(card, radius))
            {
                using var bg = new LinearGradientBrush(card, Color.FromArgb(232, 0x22, 0x2E, 0x4A), Color.FromArgb(232, 0x12, 0x19, 0x2B), 90f);
                g.FillPath(bg, path);
                using var border = new Pen(Color.FromArgb(60, 0xFF, 0xD9, 0x8A), 1f * scale);
                g.DrawPath(border, path);
            }

            float x = card.X + pad, y = card.Y + pad;
            float logoSize = 20 * scale;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(Logo, x, y + (headerH - logoSize) / 2, logoSize, logoSize);
            using (var b = new SolidBrush(Color.FromArgb(0xFF, 0xD9, 0x8A)))
                g.DrawString(dateStr, dateFont, b, x + logoSize + 8 * scale, y + (headerH - dateFont.GetHeight(g)) / 2);
            y += headerH + gap;

            using (var b = new SolidBrush(Color.FromArgb(0xFF, 0xF7, 0xEA)))
                g.DrawString(v.Texto, verseFont, b, new RectangleF(x, y, textW, verseSize.Height + 4));
            y += verseSize.Height + gap;

            using (var b = new SolidBrush(Color.FromArgb(0xE8, 0xB2, 0x4A)))
                g.DrawString(v.Referencia, refFont, b, card.Right - pad - refSize.Width, y);

            return bmp;
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
