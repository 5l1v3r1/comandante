using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Comandante
{
    public class EnvironmentService
    {
        public Environments Create()
        {
            Environments info = new Environments();
            info.CurrentDirectory = Environment.CurrentDirectory;
            info.CommandLine = Environment.CommandLine;
            info.CommandLineArgs = Environment.GetCommandLineArgs();
            info.MachineName = Environment.MachineName;
            info.OSVersion = Environment.OSVersion?.ToString();
            info.ProcessorCount = Environment.ProcessorCount;
            info.WorkingSet = Environment.WorkingSet;
            info.Version = Environment.Version?.ToString();
            info.SystemDirectory = Environment.SystemDirectory;
            info.SystemPageSize = Environment.SystemPageSize;
            info.UserName = Environment.UserName;
            info.UserInteractive = Environment.UserInteractive;
            info.UserDomainName = Environment.UserDomainName;
            info.Is64BitProcess = Environment.Is64BitProcess;
            info.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            info.EnvironmentVariables = new Dictionary<string, string>();
            var variables = Environment.GetEnvironmentVariables();
            foreach(var key in variables.Keys)
            {
                info.EnvironmentVariables.Add(key?.ToString(), variables[key]?.ToString());
            }
            
            return info;
        }
    }
    public class Environments
    {
        public string CommandLine;
        public string MachineName;
        public string OSVersion;
        public int ProcessorCount;
        public long WorkingSet;
        public string Version;
        public string SystemDirectory;
        public int SystemPageSize;
        public string UserName;
        public bool UserInteractive;
        public string UserDomainName;
        public string CurrentDirectory;
        public bool Is64BitProcess;
        public bool Is64BitOperatingSystem;
        public string[] CommandLineArgs = new string[0];
        public Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();
    }

    public class ProcessService
    {
        public ProcessInfo GetProcessInfo()
        {
            var currentProcess = Process.GetCurrentProcess();
            var info = new ProcessInfo();
            info.Id = currentProcess.Id;
            info.BasePriority = currentProcess.BasePriority;
            info.MachineName = currentProcess.MachineName;
            info.MainModuleFileName = currentProcess.MainModule?.FileName;
            info.MainModuleFileVersionInfo = currentProcess.MainModule?.FileVersionInfo?.ToString();
            info.MainModuleModuleName = currentProcess.MainModule?.ModuleName;
            info.MainModuleModuleMemorySize = SizeSuffix(currentProcess.MainModule?.ModuleMemorySize);
            info.PrivilegedProcessorTime = currentProcess.PrivilegedProcessorTime;
            info.StartTime = currentProcess.StartTime.ToString();
            info.TotalProcessorTime = currentProcess.TotalProcessorTime.ToString();
            info.UserProcessorTime = currentProcess.UserProcessorTime.ToString();
            info.ThreadsCount = currentProcess.Threads.Count.ToString();
            info.VirtualMemorySize = SizeSuffix(currentProcess.VirtualMemorySize);
            info.VirtualMemorySize64 = SizeSuffix(currentProcess.VirtualMemorySize64);
            info.PeakVirtualMemorySize = SizeSuffix(currentProcess.PeakVirtualMemorySize);
            info.PeakVirtualMemorySize64 = SizeSuffix(currentProcess.PeakVirtualMemorySize64);
            info.PrivateMemorySize = SizeSuffix(currentProcess.PrivateMemorySize);
            info.PrivateMemorySize64 = SizeSuffix(currentProcess.PrivateMemorySize64);
            info.NonpagedSystemMemorySize = SizeSuffix(currentProcess.NonpagedSystemMemorySize);
            info.NonpagedSystemMemorySize64 = SizeSuffix(currentProcess.NonpagedSystemMemorySize64);
            info.PagedMemorySize = SizeSuffix(currentProcess.PagedMemorySize);
            info.PagedMemorySize64 = SizeSuffix(currentProcess.PagedMemorySize64);
            info.PagedSystemMemorySize = SizeSuffix(currentProcess.PagedSystemMemorySize);
            info.PagedSystemMemorySize64 = SizeSuffix(currentProcess.PagedSystemMemorySize64);
            info.PeakPagedMemorySize = SizeSuffix(currentProcess.PeakPagedMemorySize);
            info.PeakPagedMemorySize64 = SizeSuffix(currentProcess.PeakPagedMemorySize64);
           
            return info;
        }

        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(long? value, int decimalPlaces = 1)
        {
            if (value.HasValue)
                return SizeSuffix(value.Value, decimalPlaces);
            else
                return null;
        }
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }

    public class ProcessInfo
    {
        public string VirtualMemorySize;
        public string VirtualMemorySize64;
        public string PeakVirtualMemorySize;
        public string PeakVirtualMemorySize64;
        public string PrivateMemorySize;
        public string PrivateMemorySize64;
        public string NonpagedSystemMemorySize;
        public string NonpagedSystemMemorySize64;
        public string PagedMemorySize;
        public string PagedMemorySize64;
        public string PagedSystemMemorySize;
        public string PagedSystemMemorySize64;
        public string PeakPagedMemorySize;
        public string PeakPagedMemorySize64;
        public int Id;
        public int BasePriority;
        public string MachineName;
        public string MainModuleFileName;
        public string MainModuleFileVersionInfo;
        public string MainModuleModuleName;
        public string MainModuleModuleMemorySize;
        public TimeSpan PrivilegedProcessorTime;
        public object StartTime;
        public object TotalProcessorTime;
        public object UserProcessorTime;
        public object ThreadsCount;
    }
}
