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
        }

        public void SetAlwaysOnTop(bool on)
        {
            _alwaysOnTop = on;
            _settings.AlwaysOnTop = on;
            _settings.Save();
            ApplyZOrder();
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
            float scale = DeviceDpi / 96f;
            using var bmp = CardRenderer.Render(Current, _shownDate, scale);
            Size = bmp.Size;
            SetBitmap(bmp);
        }

        private void PlaceWindow()
        {
            var wa = Screen.PrimaryScreen!.WorkingArea;
            int left = _settings.Left ?? wa.Right - Width - 16;
            int top = _settings.Top ?? wa.Top + 16;
            var visible = SystemInformation.VirtualScreen;
            if (left < visible.Left || left > visible.Right - 60 || top < visible.Top || top > visible.Bottom - 60)
            { left = wa.Right - Width - 16; top = wa.Top + 16; }
            Location = new Point(left, top);
            Redraw();
        }

        private void OnDrag(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            _settings.Left = Left; _settings.Top = Top; _settings.Save();
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
            else if (m.Msg == WM_DPICHANGED)
            {
                base.WndProc(ref m);
                Redraw();
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
        private const int WM_WINDOWPOSCHANGING = 0x0046, WM_NCLBUTTONDOWN = 0xA1, HTCAPTION = 2, WM_DPICHANGED = 0x02E0, ULW_ALPHA = 2;
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
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
