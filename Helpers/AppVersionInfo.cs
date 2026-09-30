using System;
using System.Reflection;

namespace Booking.Helpers
{
    public static class AppVersionInfo
    {
        /// <summary>
        /// Reads the project version dynamically from the assembly metadata populated by Booking.csproj &lt;Version&gt;.
        /// </summary>
        public static string Version
        {
            get
            {
                try
                {
                    var assembly = typeof(AppVersionInfo).Assembly;

                    // 1. Check InformationalVersion (corresponds directly to <Version> tag in csproj)
                    var infoVerAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                    if (!string.IsNullOrWhiteSpace(infoVerAttr?.InformationalVersion))
                    {
                        var cleanVer = infoVerAttr.InformationalVersion.Split('+')[0].Trim();
                        if (!string.IsNullOrEmpty(cleanVer))
                        {
                            return cleanVer;
                        }
                    }

                    // 2. Check FileVersion
                    var fileVerAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                    if (!string.IsNullOrWhiteSpace(fileVerAttr?.Version))
                    {
                        var cleanFileVer = fileVerAttr.Version.Trim();
                        if (!string.IsNullOrEmpty(cleanFileVer))
                        {
                            return cleanFileVer;
                        }
                    }

                    // 3. Check Assembly Version
                    var asmVersion = assembly.GetName().Version;
                    if (asmVersion != null)
                    {
                        return $"{asmVersion.Major}.{asmVersion.Minor}.{asmVersion.Build}";
                    }
                }
                catch
                {
                    // Fallback to default
                }

                return "1.0.2";
            }
        }
    }
}
