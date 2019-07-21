using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Debug4MvcNetCore
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
}
