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
                string path = Path.Combine(Settings.Folder, $"tela-de-bloqueio-{date:yyyyMMdd}.png");
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
                glowPath.AddEllipse(W / 2f - r, H * 0.62f - r, 2 * r, 2 * r);
                using var glow = new PathGradientBrush(glowPath)
                {
                    CenterColor = Color.FromArgb(70, 0xFF, 0xD9, 0x8A),
                    SurroundColors = new[] { Color.FromArgb(0, 0xFF, 0xD9, 0x8A) }
                };
                g.FillPath(glow, glowPath);
            }

            // Cartão do versículo, maior, um pouco abaixo do meio (o relógio fica na parte de cima).
            float cardScale = 2.1f * s;
            using var card = CardRenderer.Render(v, date, cardScale, 520);
            float x = (W - card.Width) / 2f, y = H * 0.60f - card.Height / 2f;
            g.DrawImage(card, x, y, card.Width, card.Height);
            return bmp;
        }
    }
}
