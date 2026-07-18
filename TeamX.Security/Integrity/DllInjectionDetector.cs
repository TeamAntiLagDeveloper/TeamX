using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TeamX.Security.AntiInjection;

public static class DllInjectionDetector
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll")]
    private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [DllImport("kernel32.dll")]
    private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint TH32CS_SNAPMODULE = 0x00000008;

    [StructLayout(LayoutKind.Sequential)]
    private struct MODULEENTRY32
    {
        public uint dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;
    }

    /// <summary>
    /// Lista de DLLs suspeitas (comuns em cheats/injeções)
    /// </summary>
    private static readonly HashSet<string> SuspiciousDlls = new(StringComparer.OrdinalIgnoreCase)
    {
        "mono.dll", "x64dbg.dll", "inject", "cheat", "hook", "speedhack",
        "reclass", "extreme", "trainer", "bypass", "internal", "external",
        "dbghelp.dll", "version.dll" // DLLs frequentemente usadas em injeções
    };

    public static bool DetectInjectedDlls()
    {
        try
        {
            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, (uint)Process.GetCurrentProcess().Id);
            if (snapshot == IntPtr.Zero) return false;

            MODULEENTRY32 moduleEntry = new MODULEENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32)) };

            if (Module32First(snapshot, ref moduleEntry))
            {
                do
                {
                    string moduleName = moduleEntry.szModule;

                    if (IsSuspiciousModule(moduleName))
                    {
                        CloseHandle(snapshot);
                        return true;
                    }

                } while (Module32Next(snapshot, ref moduleEntry));
            }

            CloseHandle(snapshot);
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSuspiciousModule(string moduleName)
    {
        if (string.IsNullOrEmpty(moduleName)) return false;

        // Verifica DLLs suspeitas
        if (SuspiciousDlls.Contains(moduleName))
            return true;

        // Verifica padrões comuns de injeção
        string lower = moduleName.ToLower();
        if (lower.Contains("inject") || lower.Contains("hook") || lower.Contains("bypass"))
            return true;

        return false;
    }

    public static void StartPeriodicCheck(int intervalSeconds = 8)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (DetectInjectedDlls())
                {
                    HandleInjectionDetected();
                }
                await Task.Delay(intervalSeconds * 1000);
            }
        });
    }

    private static void HandleInjectionDetected()
    {
        try
        {

        }
        catch { }

        Environment.Exit(0);
    }
}