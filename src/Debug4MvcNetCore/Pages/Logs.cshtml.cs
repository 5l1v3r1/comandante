using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Logs : EmbeddedViewModel
    {
        private LogsService _logService = new LogsService();

        public LogsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new LogsModel();
            Model.Logs = _logService.Logs.Take(1000).ToList();
            Model.LogLevel = _logService.LogLevel;
            Model.IgnoreWebHost = _logService.IgnoreWebHost;
            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (this.HttpContext.Request.Form.ContainsKey("ClearLogs"))
                {
                    _logService.ClearLogs();
                    this.HttpContext.Response.Redirect("/debug/Logs");
                }
                if (this.HttpContext.Request.Form.ContainsKey("LogLevel"))
                {
                    var logLevel = int.Parse(this.HttpContext.Request.Form["LogLevel"]);
                    _logService.LogLevel = (LogLevel)logLevel;
                    this.HttpContext.Response.Redirect("/debug/Logs");
                }
                if (this.HttpContext.Request.Form.ContainsKey("IgnoreWebHost"))
                {
                    var ignoreWebHost = bool.Parse(this.HttpContext.Request.Form["IgnoreWebHost"]);
                    _logService.IgnoreWebHost = ignoreWebHost;
                    this.HttpContext.Response.Redirect("/debug/Logs");
                }
            }
        }
    }

    public class LogsModel
    {
        public IEnumerable<LogEntry> Logs { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool IgnoreWebHost { get; set; }

    }

}
