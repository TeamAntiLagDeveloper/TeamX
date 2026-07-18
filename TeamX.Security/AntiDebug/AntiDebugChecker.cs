using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TeamX.Security.AntiDebug;

public static class AntiDebugChecker
{
    [DllImport("kernel32.dll")]
    private static extern bool IsDebuggerPresent();

    public static void Check()
    {
        if (Debugger.IsAttached || IsDebuggerPresent())
        {
            Environment.Exit(0);
        }

        // Detecta ferramentas comuns
        string[] tools = { "dnSpy", "x64dbg", "ollydbg", "ida", "ghidra" };
        foreach (var process in Process.GetProcesses())
        {
            if (tools.Any(t => process.ProcessName.ToLower().Contains(t)))
            {
                Environment.Exit(0);
            }
        }
    }
}