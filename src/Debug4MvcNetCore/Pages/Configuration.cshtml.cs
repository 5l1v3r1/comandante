using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Debug4MvcNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Configuration : EmbeddedViewModel
    {
        public ConfigurationModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new ConfigurationModel();

            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.InvariantCultureIgnoreCase))
            {
                BindModel();
                if (Model.IsValid() == false)
                    return;

                ConfigurationInfo.RequestLogLevel = Model.RequestLogLevel.Value;
                ConfigurationInfo.MaxNumberOfRequests = Model.MaxNumberOfRequests.Value;
                ConfigurationInfo.KeepWebHostLogs = Model.KeepWebHostLogs;
                ConfigurationInfo.MaxNumberOfWebHostLogs = Model.MaxNumberOfWebHostLogs.Value;
                this.HttpContext.Response.Redirect("/debug/Configuration");
            }
            else
            {
                Model.RequestLogLevel = ConfigurationInfo.RequestLogLevel;
                Model.MaxNumberOfRequests = ConfigurationInfo.MaxNumberOfRequests;
                Model.KeepWebHostLogs = ConfigurationInfo.KeepWebHostLogs;
                Model.MaxNumberOfWebHostLogs = ConfigurationInfo.MaxNumberOfWebHostLogs;
            }
        }

        private void BindModel()
        {
            if (this.HttpContext.Request.Form.ContainsKey("RequestLogLevel"))
                Model.RequestLogLevel = (RequestLogLevel)ParseInt(this.HttpContext.Request.Form["RequestLogLevel"]);

            if (this.HttpContext.Request.Form.ContainsKey("MaxNumberOfRequests"))
                Model.MaxNumberOfRequests = ParseInt(this.HttpContext.Request.Form["MaxNumberOfRequests"]);

            if (this.HttpContext.Request.Form.ContainsKey("KeepWebHostLogs"))
                Model.KeepWebHostLogs = bool.Parse(this.HttpContext.Request.Form["KeepWebHostLogs"]);

            if (this.HttpContext.Request.Form.ContainsKey("MaxNumberOfWebHostLogs"))
                Model.MaxNumberOfWebHostLogs = ParseInt(this.HttpContext.Request.Form["MaxNumberOfWebHostLogs"]);
        }

        private int? ParseInt(string text)
        {
            int number;
            if (int.TryParse(text, out number))
                return number;
            return null;
        }
    }

    public class ConfigurationModel
    {
        public int? MaxNumberOfRequests;
        public RequestLogLevel? RequestLogLevel;
        public bool KeepWebHostLogs;
        public int? MaxNumberOfWebHostLogs;

        public string Error { get; set; }

        public bool IsValid()
        {
            if (RequestLogLevel.HasValue == false)
            {
                Error = "Field 'Request Log Level' is reqired.";
                return false;
            }

            if (MaxNumberOfRequests.HasValue == false)
            {
                Error = "Field 'Max number of requests kept in memory' is reqired.";
                return false;
            }

            if (KeepWebHostLogs)
            {
                if (MaxNumberOfWebHostLogs.HasValue == false)
                {
                    Error = "Field 'Max number of logs from WebHost kept in memory' is reqired.";
                    return false;
                }
            }
            return true;
        }
    }

}
