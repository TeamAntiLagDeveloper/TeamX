using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TeamX.Security.AntiInjection;


public static class SecurityMonitor
{
    private static bool _blocked;

    public static void Start(int intervalSeconds = 5)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (RunSecurityChecks())
                    {
                        TriggerProtection();
                        return;
                    }
                }
                catch
                {

                }


                await Task.Delay(intervalSeconds * 1000);
            }

        });
    }


    private static bool RunSecurityChecks()
    {

        if (CheckDebugger())
            return true;


        if (CheckSuspiciousProcesses())
            return true;


        if (CheckInjectedModules())
            return true;


        if (CheckThreads())
            return true;


        return false;
    }

    private static bool CheckDebugger()
    {
        try
        {
            return Debugger.IsAttached ||
                   IsDebuggerPresent();
        }
        catch
        {
            return false;
        }
    }

    private static bool CheckSuspiciousProcesses()
    {

        try
        {

            string[] blacklist =
            {
                "x64dbg",
                "ollydbg",
                "ida",
                "ida64",
                "cheatengine",
                "reclass",
                "dnspy",
                "processhacker",
                "inject",
                "trainer",
                "bypass"
            };



            int current =
                Process.GetCurrentProcess().Id;



            foreach (Process process in Process.GetProcesses())
            {

                if (process.Id == current)
                    continue;


                string name =
                    process.ProcessName
                    .ToLowerInvariant();



                if (blacklist.Any(x =>
                    name.Contains(x)))
                {
                    return true;
                }

            }

        }
        catch
        {

        }


        return false;
    }

    private static bool CheckInjectedModules()
    {

        try
        {

            Process process =
                Process.GetCurrentProcess();



            foreach (ProcessModule module in process.Modules)
            {

                string path =
                    module.FileName
                    .ToLowerInvariant();



                string[] suspicious =
                {
                    "inject",
                    "cheat",
                    "hack",
                    "hook",
                    "bypass",
                    "loader",
                    "speedhack"
                };



                if (suspicious.Any(x =>
                    path.Contains(x)))
                {
                    return true;
                }


            }

        }
        catch
        {

        }


        return false;
    }


    private static bool CheckThreads()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return false;


            Process process = Process.GetCurrentProcess();


            foreach (ProcessThread thread in process.Threads)
            {
                if (thread.ThreadState == System.Diagnostics.ThreadState.Terminated)
                    continue;


                if (thread.PriorityLevel == ThreadPriorityLevel.TimeCritical)
                {
                    return true;
                }
            }
        }
        catch
        {

        }


        return false;
    }


    private static void TriggerProtection()
    {

        if (_blocked)
            return;


        _blocked = true;



        try
        {

        }
        catch
        {

        }



        Environment.FailFast(
            "Security violation detected"
        );
    }


    [DllImport(
        "kernel32.dll")]
    private static extern bool IsDebuggerPresent();

}