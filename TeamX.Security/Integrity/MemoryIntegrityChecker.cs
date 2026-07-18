using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TeamX.Security.Integrity;

public static class MemoryIntegrityChecker
{
    [DllImport("kernel32.dll")]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string lpProcName);
    public static bool CheckMemoryIntegrity()
    {
        try
        {
            // Verificar se o código principal foi modificado em memória
            if (DetectCodeModification())
                return false;

            // Verificar hooks suspeitos
            if (DetectSuspiciousHooks())
                return false;

            // Verificar se estamos sendo debugados via memória
            if (DetectMemoryDebugging())
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IntPtr GetFunctionAddress(string functionName)
    {
        IntPtr module = GetModuleHandle(null);

        if (module == IntPtr.Zero)
            return IntPtr.Zero;

        return GetProcAddress(module, functionName);
    }

    private static bool DetectCodeModification()
    {
        try
        {
            IntPtr moduleHandle = GetModuleHandle(null);
            if (moduleHandle == IntPtr.Zero) return true;

            // Verificar checksum de uma função crítica (exemplo)
            IntPtr funcAddress = GetFunctionAddress("Main"); // substitua pela sua função

            byte[] originalBytes = new byte[32]; // tamanho da instrução a verificar
            Marshal.Copy(funcAddress, originalBytes, 0, originalBytes.Length);

            // Comparar com bytes esperados (você deve salvar os bytes originais)
            // Se diferente → código foi alterado em memória
            return false; // placeholder
        }
        catch
        {
            return true;
        }
    }

    private static bool DetectSuspiciousHooks()
    {
        // Detectar se funções importantes foram hookadas (ex: IsDebuggerPresent)
        try
        {
            var process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName?.ToLower().Contains("dbg") == true ||
                    module.ModuleName?.ToLower().Contains("inject") == true)
                {
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    private static bool DetectMemoryDebugging()
    {
        // Detecta padrões de debuggers na memória
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            if (currentProcess.Handle == IntPtr.Zero) return true;

            // Verifica se o processo tem flags de debug
            return false; // expandir conforme necessário
        }
        catch
        {
            return true;
        }
    }

    public static void StartPeriodicMemoryCheck(int intervalSeconds = 5)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (!CheckMemoryIntegrity())
                {
                    HandleMemoryTampering();
                }
                await Task.Delay(intervalSeconds * 1000);
            }
        });
    }

    private static void HandleMemoryTampering()
    {
        try
        {
            Environment.Exit(0);
        }
        catch
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}