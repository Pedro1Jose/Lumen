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
            using var refFont = new Font("Segoe UI Semibold", 13.5f * scale, FontStyle.Regular, GraphicsUnit.Pixel);
            string refStr = "— " + v.Referencia;
            float textW = W - 2 * pad;

            SizeF verseSize, refSize;
            using (var tmp = new Bitmap(1, 1))
            using (var g0 = Graphics.FromImage(tmp))
            {
                g0.TextRenderingHint = TextRenderingHint.AntiAlias;
                verseSize = g0.MeasureString(v.Texto, verseFont, (int)textW, StringFormat.GenericTypographic);
                refSize = g0.MeasureString(refStr, refFont, (int)textW, StringFormat.GenericTypographic);
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
            using (var lineShadow = new Pen(Color.FromArgb(120, 10, 14, 24), 3.5f * scale))
                g.DrawLine(lineShadow, x + logo + 10 * scale, y + logo / 2, x + logo + 10 * scale + 60 * scale, y + logo / 2);
            using (var line = new Pen(Color.FromArgb(255, 0xFF, 0xCC, 0x5C), 1.6f * scale))
                g.DrawLine(line, x + logo + 10 * scale, y + logo / 2, x + logo + 10 * scale + 60 * scale, y + logo / 2);
            y += logo + headerGap;

            var rect = new RectangleF(x, y, textW, verseSize.Height + 12 * scale);
            DrawSoft(g, v.Texto, verseFont, rect, Color.FromArgb(245, 0xFF, 0xF7, 0xEA), scale);
            y += verseSize.Height + gap;
            DrawSoft(g, refStr, refFont, new RectangleF(x, y, textW, refSize.Height + 8 * scale), Color.FromArgb(0xFF, 0xCC, 0x5C), scale);
            return bmp;
        }

        // Texto com um contorno escuro suave por baixo: fica legível tanto em fundo claro quanto escuro.
        private static void DrawSoft(Graphics g, string text, Font font, RectangleF r, Color color, float scale)
        {
            using var path = new GraphicsPath();
            using (var sf = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.None })
            {
                sf.FormatFlags &= ~StringFormatFlags.NoWrap;
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, r, sf);
            }
            using (var halo = new Pen(Color.FromArgb(70, 10, 14, 24), 5f * scale) { LineJoin = LineJoin.Round })
                g.DrawPath(halo, path);
            using (var edge = new Pen(Color.FromArgb(150, 10, 14, 24), 2.2f * scale) { LineJoin = LineJoin.Round })
                g.DrawPath(edge, path);
            using var b = new SolidBrush(color);
            g.FillPath(b, path);
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
