using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Requests : EmbeddedViewModel
    {
        private RequestsService _requestsService = new RequestsService();
        public RequestsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new RequestsModel();
            Model.Requests = _requestsService.Requests;
            Model.RequestLevel = _requestsService.RequestLevel;

            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (this.HttpContext.Request.Form.ContainsKey("ClearLogs"))
                {
                    _requestsService.ClearRequests();
                    this.HttpContext.Response.Redirect("/debug/Requests");
                }
                if (this.HttpContext.Request.Form.ContainsKey("RequestLevel"))
                {
                    var logLevel = int.Parse(this.HttpContext.Request.Form["RequestLevel"]);
                    _requestsService.RequestLevel = (RequestLevel)logLevel;
                    this.HttpContext.Response.Redirect("/debug/Requests");
                }
            }
        }
    }

    public class RequestsModel
    {
        public IEnumerable<Debug4MvcNetCore.RequestInfo> Requests { get; internal set; }
        public RequestLevel RequestLevel { get; set; }
    }

}
