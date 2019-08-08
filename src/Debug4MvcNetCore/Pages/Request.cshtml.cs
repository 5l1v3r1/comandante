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
    public class Request : EmbeddedViewModel
    {
        public RequestModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            var logsService = new RequestsService();
            Model = new RequestModel();

            if (this.HttpContext.Request.Query.ContainsKey("TraceIdentifier"))
            {
                var traceIdentifier = this.HttpContext.Request.Query["TraceIdentifier"].ToString().Trim();
                Model.Request = new RequestsService().AllRequests.FirstOrDefault(x => x.TraceIdentifier == traceIdentifier);
                Model.TraceIdentifier = traceIdentifier;
            }
            return await View();
        }
    }

    public class RequestModel
    {
        public string TraceIdentifier { get; set; }
        public RequestResponseInfo Request { get; set; }
    }

}
