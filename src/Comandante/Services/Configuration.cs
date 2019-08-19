using System;
using System.Collections.Generic;
using System.Text;

namespace Comandante.Services
{
    public class ConfigurationInfo
    {
        public static bool EnableRequestLogs { get; set; } = true;
        public static bool EnableRequestLogsOnlyIfError { get; set; } = false;
        public static bool EnableRequestLogsOnlyIfAspMvc { get; set; } = true;
        public static int MaxNumberOfRequestsLogs { get; set; } = 2000;
        public static bool EnableWebHostLogs { get; set; } = false;
        public static int MaxNumberOfWebHostLogs { get; set; } = 2000;
    }
    public enum RequestLogLevel : int
    {
        OnlyIfError = 2,
        All = 1,
        Node = 0
    }

}

