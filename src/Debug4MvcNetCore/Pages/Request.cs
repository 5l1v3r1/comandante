using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Request : EmbeddedPageModel
    {
        public RequestModel Model { get; set; }

        public override async Task InitPage()
        {
            var logsService = new LogsService();
            Model = new RequestModel();
            var traceIdentifier = this.HttpContext.Request.Query["TraceIdentifier"].ToString();
            Model.Logs = logsService.Logs.Where(x => x.TraceIdentifier == traceIdentifier);
            Model.DebugInfo = logsService.Requests.FirstOrDefault(x => x.TraceIdentifier == traceIdentifier);
        }
    }

    public class RequestModel
    {
        public DebugInfo DebugInfo { get; set; }
        public IEnumerable<LogEntry> Logs { get; internal set; }
    }

}
