// Code according to https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed

using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CheckDotNetVersion
{
  class Program
  {
    static void Main(string[] args)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        DotNetVersion.CheckDotNetVersionOneToFour();
        DotNetVersion.CheckDotNetVersionFour();
      }
      else
      {
        string os = "";
        if (OperatingSystem.IsLinux())
          os = "Linux";
        if (OperatingSystem.IsMacOS())
          os = "MacOS";
        if (os == "")
          os = "=undetected=";

        Console.WriteLine($"No Windows OS version detected! Your OS is based on {os}");
      }
    }
  }

  class DotNetVersion
  {
    private static void PrintHeadline(string headline)
    {
      int headlineLength = headline.Length;

      for (int i = 0; i < headlineLength + 4; i++)
      {
        Console.Write("#");
      }
      Console.WriteLine();

      Console.WriteLine($"# {headline} #");

      for (int i = 0; i < headlineLength + 4; i++)
      {
        Console.Write("#");
      }
      Console.WriteLine();
    }

    public static void CheckDotNetVersionOneToFour(string headline = "")
    {
      if (headline == "")
      {
        headline = "Detect .NET Framework 1.0 through 4.0";
      }
      PrintHeadline(headline);

      // Open the registry key for the .NET Framework entry.
      using (RegistryKey ndpKey =
              RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).
              OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
      {
        foreach (var versionKeyName in ndpKey.GetSubKeyNames())
        {
          // Skip .NET Framework 4.5 version information.
          if (versionKeyName == "v4")
          {
            continue;
          }

          if (versionKeyName.StartsWith("v"))
          {
            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);

            // Get the .NET Framework version value.
            var name = (string)versionKey.GetValue("Version", "");
            // Get the service pack (SP) number.
            var sp = versionKey.GetValue("SP", "").ToString();

            // Get the installation flag.
            var install = versionKey.GetValue("Install", "").ToString();
            if (string.IsNullOrEmpty(install))
            {
              // No install info; it must be in a child subkey.
              Console.WriteLine($"{versionKeyName}  {name}");
            }
            else if (install == "1")
            {
              // Install = 1 means the version is installed.

              if (!string.IsNullOrEmpty(sp))
              {
                Console.WriteLine($"{versionKeyName}  {name}  SP{sp}");
              }
              else
              {
                Console.WriteLine($"{versionKeyName}  {name}");
              }
            }

            if (!string.IsNullOrEmpty(name))
            {
              continue;
            }
            // else print out info from subkeys...

            // Iterate through the subkeys of the version subkey.
            foreach (var subKeyName in versionKey.GetSubKeyNames())
            {
              RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
              name = (string)subKey.GetValue("Version", "");
              if (!string.IsNullOrEmpty(name))
                sp = subKey.GetValue("SP", "").ToString();

              install = subKey.GetValue("Install", "").ToString();
              if (string.IsNullOrEmpty(install))
              {
                // No install info; it must be later.
                Console.WriteLine($"{versionKeyName}  {name}");
              }
              else if (install == "1")
              {
                if (!string.IsNullOrEmpty(sp))
                {
                  Console.WriteLine($"{subKeyName}  {name}  SP{sp}");
                }
                else
                {
                  Console.WriteLine($"  {subKeyName}  {name}");
                }
              }
            }
          }
        }
      }
    }

    public static void CheckDotNetVersionFour(string headline = "")
    {
      if (headline == "")
      {
        headline = "Detect .NET Framework 4.5 and later versions";
      }
      PrintHeadline(headline);

      const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

      using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
      {
        if (ndpKey != null && ndpKey.GetValue("Release") != null)
        {
          Console.WriteLine($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}");
        }
        else
        {
          Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
        }
      }

      // Checking the version using >= enables forward compatibility.
      string CheckFor45PlusVersion(int releaseKey)
      {
        if (releaseKey > 528372)
          return ".NET Framework 4.8 or later";
        if (releaseKey >= 528372)
          return ".NET Framework 4.8 on Windows 10 May 2020 Update and Windows 10 October 2020 Update";
        if (releaseKey >= 528049)
          return ".NET Framework 4.8 on all Windows operating systems (including other Windows 10 operating systems) other than Windows 10 May 2019 Update, Windows 10 November 2019 Update, Windows 10 May 2020 Update and Windows 10 October 2020 Update";
        if (releaseKey >= 528040) // return "4.8 or later";
          return ".NET Framework 4.8 on Windows 10 May 2019 Update and Windows 10 November 2019 Update";
        if (releaseKey >= 461814)
          return ".NET Framework 4.7.2 on all Windows operating systems other than Windows 10 April 2018 Update and Windows Server, version 1803";
        if (releaseKey >= 461808) // return "4.7.2";
          return ".NET Framework 4.7.2 on Windows 10 April 2018 Update and Windows Server, version 1803";
        if (releaseKey >= 461310)
          return ".NET Framework 4.7.1 on all Windows operating systems (including other Windows 10 operating systems) other than Windows 10 Fall Creators Update and Windows Server, version 1709";
        if (releaseKey >= 461308) // return "4.7.1";
          return ".NET Framework 4.7.1 on Windows 10 Fall Creators Update and Windows Server, version 1709";
        if (releaseKey >= 460805)
          return ".NET Framework 4.7 on all Windows operating systems (including other Windows 10 operating systems) other than Windows 10 Creators Update";
        if (releaseKey >= 460798) // return "4.7";
          return ".NET Framework 4.7 on Windows 10 Creators Update";
        if (releaseKey >= 394806)
          return ".NET Framework 4.6.2 on all Windows operating systems (including other Windows 10 operating systems) other than Windows 10 Anniversary Update and Windows Server 2016";
        if (releaseKey >= 394802) // return "4.6.2";
          return ".NET Framework 4.6.2 on Windows 10 Anniversary Update and Windows Server 2016";
        if (releaseKey >= 394271)
          return ".NET Framework 4.6.1 on all Windows operating systems (including Windows 10) other than Windows 10 November Update Systems";
        if (releaseKey >= 394254) // return "4.6.1";
          return ".NET Framework 4.6.1 on Windows 10 November Update systems";
        if (releaseKey >= 393297)
          return ".NET Framework 4.6 on all Windows operating systems other than Windows 10";
        if (releaseKey >= 393295) // return "4.6";
          return ".NET Framework 4.6 on Windows 10";
        if (releaseKey >= 379893) // return "4.5.2";
          return ".NET Framework 4.5.2 ";
        if (releaseKey >= 378675)
          return "4.5.1 on all Windows operating systems";
        if (releaseKey >= 378758)
          return ".NET Framework 4.5.1 on all Windows operating systems other than Windows 8.1 and Windows Server 2012 R2";
        if (releaseKey >= 378675)
          return ".NET Framework 4.5.1 on Windows 8.1 and Windows Server 2012 R2";
        if (releaseKey >= 378389) // return "4.5";
          return ".NET Framework 4.5 on all Windows operating systems";
        // This code should never execute. A non-null release key should mean
        // that 4.5 or later is installed.
        return "No 4.5 or later version detected";
      }
    }
  }

  public static class OperatingSystem
  {
    public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
  }
}

