using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Lumen
{
    // Atualização sem fechar: se aparecer um "Lumen-novo.exe" ao lado do Lumen.exe,
    // o Lumen se fecha, troca o arquivo e abre a versão nova sozinho.
    public static class Updater
    {
        private static System.Windows.Forms.Timer? _timer;
        private static bool _started;

        private static string Exe => Environment.ProcessPath!;
        private static string NewExe => Path.Combine(Path.GetDirectoryName(Exe)!, "Lumen-novo.exe");

        public static void Start()
        {
            // Confere a cada 5 segundos.
            _timer = new System.Windows.Forms.Timer { Interval = 5000 };
            _timer.Tick += (_, _) => TryApply();
            _timer.Start();
            TryApply();
        }

        private static void TryApply()
        {
            if (_started || !File.Exists(NewExe)) return;
            try
            {
                // Só continua quando o arquivo terminou de ser copiado.
                using (var f = new FileStream(NewExe, FileMode.Open, FileAccess.Read, FileShare.None))
                    if (f.Length < 100_000) return;
                if (DateTime.Now - File.GetLastWriteTime(NewExe) < TimeSpan.FromSeconds(3)) return;
            }
            catch { return; }

            _started = true;
            string script = Path.Combine(Path.GetTempPath(), "lumen-atualizar.cmd");
            File.WriteAllText(script,
$@"@echo off
set n=0
:espera
timeout /t 1 /nobreak >nul
set /a n+=1
move /y ""{NewExe}"" ""{Exe}"" >nul 2>&1 && goto abre
if %n% lss 30 goto espera
:abre
start """" ""{Exe}""
del ""%~f0""
");
            Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{script}\"") { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
            Application.Exit();
        }
    }
}
