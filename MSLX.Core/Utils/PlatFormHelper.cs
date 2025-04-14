using System.Runtime.InteropServices;

namespace MSLX.Core.Utils;

public class PlatFormHelper
{
    public static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "MacOS";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "Linux";

        return "unknown";
    }

    public static string GetOsArch()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return "arm64";
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64) return "amd64";

        return "unknown";
    }
}