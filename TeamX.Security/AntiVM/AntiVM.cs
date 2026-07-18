using System.Management;

namespace TeamX.Security.AntiVM;

public static class AntiVM
{
    public static bool IsRunningInVM()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                string model = obj["Model"]?.ToString() ?? "";

                if (manufacturer.Contains("VMware") ||
                    manufacturer.Contains("Virtual") ||
                    model.Contains("Virtual"))
                {
                    return true;
                }
            }
        }
        catch { }

        return false;
    }
}