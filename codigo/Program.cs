using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Lumen
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            using var mutex = new Mutex(true, "Lumen.VersiculoDoDia.SingleInstance", out bool isNew);
            if (!isNew) return;

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var settings = Settings.Load();
            if (!settings.FirstRunDone)
            {
                try { Startup.Set(true); } catch { }
                settings.FirstRunDone = true;
                settings.Save();
            }

            var widget = new WidgetForm(settings);
            using var tray = CreateTray(widget, settings);
            widget.Show();
            Updater.Start();
            Application.Run();
            tray.Visible = false;
        }

        private static NotifyIcon CreateTray(WidgetForm widget, Settings settings)
        {
            using var icoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("lumen.ico")!;
            var menu = new ContextMenuStrip();

            var mostrar = new ToolStripMenuItem("Mostrar o widget", null, (_, _) => { widget.Show(); widget.ShowToday(); });
            var ocultar = new ToolStripMenuItem("Ocultar o widget", null, (_, _) => widget.Hide());
            var copiar = new ToolStripMenuItem("Copiar versículo", null, (_, _) =>
                Clipboard.SetText($"{widget.Current.Texto} ({widget.Current.Referencia})"));
            var topo = new ToolStripMenuItem("Sempre por cima das janelas") { CheckOnClick = true, Checked = settings.AlwaysOnTop };
            topo.CheckedChanged += (_, _) => widget.SetAlwaysOnTop(topo.Checked);
            var iniciar = new ToolStripMenuItem("Iniciar com o Windows") { CheckOnClick = true };
            try { iniciar.Checked = Startup.IsEnabled(); } catch { }
            iniciar.CheckedChanged += (_, _) => { try { Startup.Set(iniciar.Checked); } catch { } };
            var capitulo = new ToolStripMenuItem("Ler o capítulo inteiro", null, (_, _) => ChapterForm.ShowFor(widget.Current));
            var tamanho = new ToolStripMenuItem("Tamanho do texto");
            foreach (var (nome, valor) in new[] { ("Pequeno", "Pequeno"), ("Médio", "Medio"), ("Grande", "Grande") })
            {
                var item = new ToolStripMenuItem(nome) { Checked = settings.TextSize == valor, Tag = valor };
                item.Click += (_, _) =>
                {
                    foreach (ToolStripMenuItem i in tamanho.DropDownItems) i.Checked = i == item;
                    widget.SetTextSize(valor);
                };
                tamanho.DropDownItems.Add(item);
            }
            var bloqueio = new ToolStripMenuItem("Versículo na tela de bloqueio") { CheckOnClick = true, Checked = settings.LockScreen };
            bloqueio.CheckedChanged += async (_, _) =>
            {
                settings.LockScreen = bloqueio.Checked;
                settings.Save();
                if (bloqueio.Checked && !await LockScreenImage.UpdateAsync(widget.Current, DateTime.Today))
                    MessageBox.Show("Não consegui trocar a imagem da tela de bloqueio.\n\n" + LockScreenImage.LastError, "Lumen");
            };
            var sair = new ToolStripMenuItem("Sair", null, (_, _) => Application.Exit());

            menu.Items.AddRange(new ToolStripItem[] { mostrar, ocultar, capitulo, copiar, new ToolStripSeparator(), tamanho, topo, bloqueio, iniciar, new ToolStripSeparator(), sair });

            var tray = new NotifyIcon { Icon = new Icon(icoStream), Visible = true, ContextMenuStrip = menu };
            void UpdateTip() { var t = "Lumen · " + widget.Current.Referencia; tray.Text = t.Length > 63 ? "Lumen" : t; }
            widget.VerseChanged += UpdateTip;
            UpdateTip();
            tray.DoubleClick += (_, _) => { widget.Show(); widget.ShowToday(); };
            return tray;
        }
    }
}
