using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TeamX.Security.AntiDebug;

public static class DebuggerDetector
{
    [DllImport("kernel32.dll")]
    private static extern bool IsDebuggerPresent();

    [DllImport("kernel32.dll")]
    private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

    public static void FullCheck()
    {
        if (IsDebugging())
        {
            HandleDebuggerDetected();
        }
    }

    private static bool IsDebugging()
    {
        try
        {
            // Método 1: Debugger.IsAttached (mais simples)
            if (Debugger.IsAttached)
                return true;

            // Método 2: IsDebuggerPresent API
            if (IsDebuggerPresent())
                return true;

            // Método 3: CheckRemoteDebuggerPresent
            bool isDebuggerPresent = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
            if (isDebuggerPresent)
                return true;

            // Método 4: Detectar threads de debug
            if (DetectDebugThreads())
                return true;

            // Método 5: Detectar ferramentas comuns
            if (DetectCommonDebugTools())
                return true;
        }
        catch { }

        return false;
    }

    private static bool DetectDebugThreads()
    {
        foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
        {
            if (thread.ThreadState == System.Diagnostics.ThreadState.Wait &&
                thread.WaitReason.ToString().Contains("Debug"))
            {
                return true;
            }
        }
        return false;
    }

    private static bool DetectCommonDebugTools()
    {
        string[] debugTools =
        {
            "dnspy", "x64dbg", "x32dbg", "ollydbg", "ida", "ghidra",
            "windbg", "fiddler", "httpdebugger", "procmon", "debugview"
        };

        foreach (var process in Process.GetProcesses())
        {
            string name = process.ProcessName.ToLower();
            if (debugTools.Any(tool => name.Contains(tool)))
                return true;
        }
        return false;
    }

    private static void HandleDebuggerDetected()
    {
        // Comportamento ao detectar debugger
        try
        {
            // Opção 1: Fechar silenciosamente
            Environment.Exit(0);

            // Opção 2: Mensagem falsa (engana o cracker)
            // MessageBox.Show("Erro interno. Contate o suporte.", "TeamX", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Opção 3: Corromper dados (opcional e perigoso)
        }
        catch
        {
            Environment.Exit(1);
        }
    }

    // Método para chamar periodicamente
    public static async Task StartPeriodicCheckAsync(int intervalSeconds = 10)
    {
        while (true)
        {
            FullCheck();
            await Task.Delay(intervalSeconds * 1000);
        }
    }
}