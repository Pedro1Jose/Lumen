using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Lumen
{
    // Janela sem bordas, com transparência real (layered window), que mostra o cartão do dia.
    public class WidgetForm : Form
    {
        private readonly Settings _settings;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 60_000 };
        private DateTime _shownDate;
        private bool _alwaysOnTop;
        public Verse Current { get; private set; } = new("", "");
        public event Action? VerseChanged;

        public WidgetForm(Settings settings)
        {
            _settings = settings;
            _alwaysOnTop = settings.AlwaysOnTop;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Lumen";
            _timer.Tick += (_, _) => { if (DateTime.Today != _shownDate) ShowToday(); };
            _timer.Start();
            _dpiTimer.Tick += (_, _) => CheckDisplay();
            _dpiTimer.Start();
            MouseDown += OnDrag;
            SystemEvents_Resume();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                if (_alwaysOnTop) cp.ExStyle |= WS_EX_TOPMOST;
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AttachToDesktop();
            ShowToday();
            PlaceWindow();
            ApplyZOrder();
        }

        public void ShowToday()
        {
            _shownDate = DateTime.Today;
            Current = Verses.ForDate(_shownDate);
            Redraw();
            VerseChanged?.Invoke();
            if (_settings.LockScreen) _ = LockScreenImage.UpdateAsync(Current, _shownDate);
        }

        public void SetTextSize(string size)
        {
            _settings.TextSize = size;
            _settings.Save();
            PlaceWindow();
        }

        public void SetAlwaysOnTop(bool on)
        {
            _alwaysOnTop = on;
            _settings.AlwaysOnTop = on;
            _settings.Save();
            ApplyZOrder();
        }

        // Faz a área de trabalho ser a "dona" do widget. Assim o Win+D ("Mostrar área de trabalho") não o esconde.
        private void AttachToDesktop()
        {
            try
            {
                var progman = FindWindow("Progman", null);
                if (progman != IntPtr.Zero) SetWindowLongPtr(Handle, GWLP_HWNDPARENT, progman);
            }
            catch { }
        }

        private void ApplyZOrder()
        {
            if (!IsHandleCreated) return;
            SetWindowPos(Handle, _alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            if (!_alwaysOnTop) SetWindowPos(Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        private void Redraw()
        {
            if (!IsHandleCreated) return;
            float scale = CurrentScale();
            _lastScale = scale;
            _lastScreen = Screen.FromPoint(Location).Bounds;
            using var bmp = CardRenderer.Render(Current, _shownDate, scale * _settings.SizeFactor);
            Size = bmp.Size;
            SetBitmap(bmp);
        }

        private float _lastScale;
        private Rectangle _lastScreen;
        private readonly System.Windows.Forms.Timer _dpiTimer = new() { Interval = 2000 };

        // Escala real do monitor onde o widget está (100%, 125%, 150%...).
        private float CurrentScale()
        {
            try
            {
                var mon = MonitorFromPoint(new POINT { x = Left + 5, y = Top + 5 }, MONITOR_DEFAULTTONEAREST);
                if (GetDpiForMonitor(mon, 0, out uint dx, out _) == 0 && dx > 0) return dx / 96f;
            }
            catch { }
            return DeviceDpi / 96f;
        }

        // Se a escala ou a resolução mudarem (troca de monitor, ajuste nas Configurações, PC acordando), redesenha.
        private void CheckDisplay()
        {
            if (!IsHandleCreated) return;
            var screen = Screen.FromPoint(Location).Bounds;
            if (Math.Abs(CurrentScale() - _lastScale) > 0.01f || screen != _lastScreen) PlaceWindow();
        }

        private void PlaceWindow()
        {
            var visible = SystemInformation.VirtualScreen;
            bool hasSaved = _settings.Left.HasValue && _settings.Top.HasValue;
            int left = _settings.Left ?? 0, top = _settings.Top ?? 0;
            if (!hasSaved || left < visible.Left || left > visible.Right - 60 || top < visible.Top || top > visible.Bottom - 60)
            {
                var wa0 = Screen.PrimaryScreen!.WorkingArea;
                left = wa0.Right - 16; top = wa0.Top + 16; hasSaved = false;
            }
            Location = new Point(left, top);
            Redraw(); // desenha na escala do monitor certo

            // Depois de saber o tamanho novo, garante que o widget cabe inteiro na tela.
            var wa = Screen.FromPoint(new Point(left + 5, top + 5)).WorkingArea;
            if (!hasSaved) left = wa.Right - Width - 16;
            left = Math.Max(wa.Left, Math.Min(left, wa.Right - Width));
            top = Math.Max(wa.Top, Math.Min(top, wa.Bottom - Height));
            if (left != Left || top != Top) { Location = new Point(left, top); Redraw(); }
        }

        private void OnDrag(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var before = Location;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            // Se não mexeu, foi um clique: abre o capítulo inteiro.
            if (Math.Abs(Left - before.X) < 3 && Math.Abs(Top - before.Y) < 3)
            {
                ChapterForm.ShowFor(Current);
                return;
            }
            _settings.Left = Left; _settings.Top = Top; _settings.Save();
            CheckDisplay();
            ApplyZOrder();
        }

        private void SystemEvents_Resume()
        {
            // Depois de suspender/hibernar, confere se o dia mudou.
            Microsoft.Win32.SystemEvents.PowerModeChanged += (_, e) =>
            { if (e.Mode == Microsoft.Win32.PowerModes.Resume) BeginInvoke(new Action(() => { if (DateTime.Today != _shownDate) ShowToday(); })); };
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_, _) => BeginInvoke(new Action(PlaceWindow));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_WINDOWPOSCHANGING && !_alwaysOnTop)
            {
                var pos = Marshal.PtrToStructure<WINDOWPOS>(m.LParam);
                pos.hwndInsertAfter = HWND_BOTTOM;
                Marshal.StructureToPtr(pos, m.LParam, false);
            }
            else if (m.Msg == WM_TASKBARCREATED && WM_TASKBARCREATED != 0)
            {
                BeginInvoke(new Action(() => { AttachToDesktop(); ApplyZOrder(); }));
            }
            else if (m.Msg == WM_DPICHANGED || m.Msg == WM_DISPLAYCHANGE)
            {
                BeginInvoke(new Action(Redraw));
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }

        // ---- Win32 ----
        private void SetBitmap(Bitmap bmp)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero), memDc = CreateCompatibleDC(screenDc), hBmp = IntPtr.Zero, old = IntPtr.Zero;
            try
            {
                hBmp = bmp.GetHbitmap(Color.FromArgb(0));
                old = SelectObject(memDc, hBmp);
                var size = new SIZE { cx = bmp.Width, cy = bmp.Height };
                var src = new POINT();
                var dst = new POINT { x = Left, y = Top };
                var blend = new BLENDFUNCTION { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = 1 };
                UpdateLayeredWindow(Handle, screenDc, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, screenDc);
                if (hBmp != IntPtr.Zero) { SelectObject(memDc, old); DeleteObject(hBmp); }
                DeleteDC(memDc);
            }
        }

        private const int WS_EX_LAYERED = 0x80000, WS_EX_TOOLWINDOW = 0x80, WS_EX_NOACTIVATE = 0x08000000, WS_EX_TOPMOST = 0x8;
        private const int WM_WINDOWPOSCHANGING = 0x0046, WM_NCLBUTTONDOWN = 0xA1, HTCAPTION = 2, WM_DPICHANGED = 0x02E0, WM_DISPLAYCHANGE = 0x007E, MONITOR_DEFAULTTONEAREST = 2, ULW_ALPHA = 2;
        private static readonly IntPtr HWND_BOTTOM = new(1), HWND_TOPMOST = new(-1), HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOACTIVATE = 0x10;

        [StructLayout(LayoutKind.Sequential)] private struct WINDOWPOS { public IntPtr hwnd, hwndInsertAfter; public int x, y, cx, cy; public uint flags; }
        [StructLayout(LayoutKind.Sequential)] private struct POINT { public int x, y; }
        [StructLayout(LayoutKind.Sequential)] private struct SIZE { public int cx, cy; }
        [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }

        [DllImport("user32.dll")] private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint f);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        private const int GWLP_HWNDPARENT = -8;
        private static readonly int WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int RegisterWindowMessage(string msg);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr FindWindow(string? cls, string? name);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static extern IntPtr SetWindowLongPtr(IntPtr h, int i, IntPtr v);
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromPoint(POINT pt, int flags);
        [DllImport("shcore.dll")] private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
