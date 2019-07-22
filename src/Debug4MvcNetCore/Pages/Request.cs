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

        public override async Task InitView()
        {
            var logsService = new LogsService();
            Model = new RequestModel();


            if (this.HttpContext.Request.Query.ContainsKey("TraceIdentifier"))
            {
                var traceIdentifier = this.HttpContext.Request.Query["TraceIdentifier"].ToString().Trim();
                Model.Logs = new LogsService().Logs.Where(x => x?.TraceIdentifier == traceIdentifier).ToList();
                Model.Request = new RequestsService().Requests.FirstOrDefault(x => x.TraceIdentifier == traceIdentifier);
            }
        }
    }

    public class RequestModel
    {
        public RequestInfo Request { get; set; }
        public IEnumerable<LogEntry> Logs { get; internal set; }
    }

}
