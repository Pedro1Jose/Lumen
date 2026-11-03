using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lumen
{
    // Gera uma imagem com o versículo do dia e a coloca como fundo da tela de bloqueio.
    public static class LockScreenImage
    {
        public static string? LastError { get; private set; }

        public static async Task<bool> UpdateAsync(Verse v, DateTime date)
        {
            try
            {
                Directory.CreateDirectory(Settings.Folder);
                // Nome diferente a cada dia para o Windows não reaproveitar a imagem antiga do cache.
                string path = Path.Combine(Settings.Folder, $"tela-de-bloqueio-v2-{date:yyyyMMdd}.png");
                if (!File.Exists(path))
                {
                    using var img = Render(v, date);
                    img.Save(path, ImageFormat.Png);
                }
                foreach (var old in Directory.GetFiles(Settings.Folder, "tela-de-bloqueio-*.png"))
                    if (!string.Equals(old, path, StringComparison.OrdinalIgnoreCase)) { try { File.Delete(old); } catch { } }

                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                await Windows.System.UserProfile.LockScreen.SetImageFileAsync(file);
                LastError = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public static Bitmap Render(Verse v, DateTime date)
        {
            var b = Screen.PrimaryScreen!.Bounds;
            int W = Math.Max(b.Width, 1920), H = (int)Math.Round(W * (double)b.Height / b.Width);
            float s = W / 1920f;

            var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fundo azul-noite com um brilho dourado, como a chama da logo.
            using (var bg = new LinearGradientBrush(new Rectangle(0, 0, W, H), Color.FromArgb(0x24, 0x31, 0x52), Color.FromArgb(0x0B, 0x10, 0x1C), 90f))
                g.FillRectangle(bg, 0, 0, W, H);
            using (var glowPath = new GraphicsPath())
            {
                float r = 900 * s;
                glowPath.AddEllipse(W * 0.18f - r, H * 0.80f - r, 2 * r, 2 * r);
                using var glow = new PathGradientBrush(glowPath)
                {
                    CenterColor = Color.FromArgb(70, 0xFF, 0xD9, 0x8A),
                    SurroundColors = new[] { Color.FromArgb(0, 0xFF, 0xD9, 0x8A) }
                };
                g.FillPath(glow, glowPath);
            }

            // Estilo minimalista: só o texto, no canto inferior esquerdo, longe do relógio e da caixa do PIN.
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            float left = W * 0.065f, maxW = W * 0.36f, bottom = H * 0.80f;
            using var verseFont = new Font("Georgia", 30f * s, GraphicsUnit.Pixel);
            using var refFont = new Font("Segoe UI", 20f * s, FontStyle.Italic, GraphicsUnit.Pixel);
            var verseSize = g.MeasureString(v.Texto, verseFont, (int)maxW);
            var refSize = g.MeasureString("— " + v.Referencia, refFont);
            float gap = 18 * s;
            float top = bottom - verseSize.Height - gap - refSize.Height;

            // Linha dourada fina e a lamparina pequena, acima do texto.
            float logo = 28 * s;
            g.DrawImage(CardRenderer.Logo, left, top - logo - 22 * s, logo, logo);
            using (var line = new Pen(Color.FromArgb(160, 0xE8, 0xB2, 0x4A), 2f * s))
                g.DrawLine(line, left + logo + 14 * s, top - logo / 2 - 22 * s, left + logo + 14 * s + 90 * s, top - logo / 2 - 22 * s);

            var rect = new RectangleF(left, top, maxW, verseSize.Height + 4);
            using (var shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                g.DrawString(v.Texto, verseFont, shadow, new RectangleF(rect.X + 2 * s, rect.Y + 2 * s, rect.Width, rect.Height));
            using (var white = new SolidBrush(Color.FromArgb(240, 0xFF, 0xF7, 0xEA)))
                g.DrawString(v.Texto, verseFont, white, rect);
            using (var gold = new SolidBrush(Color.FromArgb(0xE8, 0xB2, 0x4A)))
                g.DrawString("— " + v.Referencia, refFont, gold, left, top + verseSize.Height + gap);
            return bmp;
        }
    }
}
