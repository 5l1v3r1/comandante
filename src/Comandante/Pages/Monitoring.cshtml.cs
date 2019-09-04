using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Comandante.Pages
{
    public class Monitoring : EmbeddedViewModel
    {
        public MonitoringModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new MonitoringModel();

            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (this.HttpContext.Request.Form.ContainsKey("ClearLogs"))
                {
                    new MonitoringService().ClearLogs();
                    this.HttpContext.Response.Redirect(@Url.Link("/ Monitoring", new { }));
                }
                if (this.HttpContext.Request.Form.ContainsKey("UpdateLogsConfiguration"))
                {
                    BindModel();
                    if (Model.IsValid() == false)
                        return await View(); ;

                    ConfigurationInfo.EnableRequestLogs = Model.EnableRequestsLogs;
                    if (ConfigurationInfo.EnableRequestLogs)
                    {
                        if (Model.MaxNumberOfRequestsLogs.HasValue)
                            ConfigurationInfo.MaxNumberOfRequestsLogs = Model.MaxNumberOfRequestsLogs.Value;
                        ConfigurationInfo.EnableRequestLogsOnlyIfAspMvc = Model.EnableRequestsLogsOnlyIfAspMvc;
                        ConfigurationInfo.EnableRequestLogsOnlyIfError = Model.EnableRequestsLogsOnlyIfError;
                    }
                    ConfigurationInfo.EnableWebHostLogs = Model.EnableWebHostLogs;
                    if (ConfigurationInfo.EnableWebHostLogs)
                    {
                        if (Model.MaxNumberOfWebHostLogs.HasValue)
                            ConfigurationInfo.MaxNumberOfWebHostLogs = Model.MaxNumberOfWebHostLogs.Value;
                    }
                    this.HttpContext.Response.Redirect(@Url.Link("/Monitoring", new { }));
                }
            }
            else
            {
                Model.EnableRequestsLogs = ConfigurationInfo.EnableRequestLogs;
                Model.EnableRequestsLogsOnlyIfError = ConfigurationInfo.EnableRequestLogsOnlyIfError;
                Model.EnableRequestsLogsOnlyIfAspMvc = ConfigurationInfo.EnableRequestLogsOnlyIfAspMvc;
                Model.MaxNumberOfRequestsLogs = ConfigurationInfo.MaxNumberOfRequestsLogs;
                Model.EnableWebHostLogs = ConfigurationInfo.EnableWebHostLogs;
                Model.MaxNumberOfWebHostLogs = ConfigurationInfo.MaxNumberOfWebHostLogs;
            }
            return await View();
        }

        private void BindModel()
        {
            if (this.HttpContext.Request.Form.ContainsKey("EnableRequestsLogs"))
                Model.EnableRequestsLogs = ParseBool(this.HttpContext.Request.Form["EnableRequestsLogs"]).GetValueOrDefault(false);

            if (this.HttpContext.Request.Form.ContainsKey("EnableRequestsLogsOnlyIfError"))
                Model.EnableRequestsLogsOnlyIfError = ParseBool(this.HttpContext.Request.Form["EnableRequestsLogsOnlyIfError"]).GetValueOrDefault(false);

            if (this.HttpContext.Request.Form.ContainsKey("EnableRequestsLogsOnlyIfAspMvc"))
                Model.EnableRequestsLogsOnlyIfAspMvc = ParseBool(this.HttpContext.Request.Form["EnableRequestsLogsOnlyIfAspMvc"]).GetValueOrDefault(false);

            if (this.HttpContext.Request.Form.ContainsKey("MaxNumberOfRequestsLogs"))
                Model.MaxNumberOfRequestsLogs = ParseInt(this.HttpContext.Request.Form["MaxNumberOfRequestsLogs"]);

            if (this.HttpContext.Request.Form.ContainsKey("EnableWebHostLogs"))
                Model.EnableWebHostLogs = ParseBool(this.HttpContext.Request.Form["EnableWebHostLogs"]).GetValueOrDefault(false);

            if (this.HttpContext.Request.Form.ContainsKey("MaxNumberOfWebHostLogs"))
                Model.MaxNumberOfWebHostLogs = ParseInt(this.HttpContext.Request.Form["MaxNumberOfWebHostLogs"]);
        }

        private int? ParseInt(string text)
        {
            int valu;
            if (int.TryParse(text, out valu))
                return valu;
            return null;
        }

        private bool? ParseBool(string text)
        {
            if (string.Equals(text, "on", StringComparison.CurrentCultureIgnoreCase))
                return true;

            bool value;
            if (bool.TryParse(text, out value))
                return value;
            return null;
        }
    }

    public class MonitoringModel
    {
        public bool EnableRequestsLogs;
        public bool EnableRequestsLogsOnlyIfError;
        public bool EnableRequestsLogsOnlyIfAspMvc;
        public int? MaxNumberOfRequestsLogs;
        public bool EnableWebHostLogs;
        public int? MaxNumberOfWebHostLogs;

        public string Error { get; set; }


        public bool IsValid()
        {
            if (EnableRequestsLogs)
            {
                if (MaxNumberOfRequestsLogs.HasValue == false)
                {
                    Error = "Field 'Max number of requests kept in memory' is reqired.";
                    return false;
                }
            }
            if (EnableWebHostLogs)
            {
                if (MaxNumberOfWebHostLogs.HasValue == false)
                {
                    Error = "Field 'Max number of WebHost logs kept in memory' is reqired.";
                    return false;
                }
            }
            return true;
        }
    }

}
