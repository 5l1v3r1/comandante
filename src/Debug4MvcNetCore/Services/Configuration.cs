using System;
using System.Collections.Generic;
using System.Text;

namespace Debug4MvcNetCore.Services
{
    public class ConfigurationInfo
    {
        public static RequestLogLevel RequestLogLevel { get; set; } = RequestLogLevel.All;
        public static int MaxNumberOfRequests { get; set; } = 2000;
        public static bool KeepWebHostLogs { get; set; } = false;
        public static int MaxNumberOfWebHostLogs { get; set; } = 2000;
    }
    public enum RequestLogLevel : int
    {
        OnlyIfError = 2,
        All = 1,
        Node = 0
    }

}

