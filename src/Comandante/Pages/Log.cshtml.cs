using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class Log : EmbeddedViewModel
    {
        private MonitoringService _requestsService = new MonitoringService();

        public LogModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new LogModel();
            if (this.HttpContext.Request.Query.ContainsKey("Id"))
                Model.Id = this.HttpContext.Request.Query["Id"].ToString().Trim();
            Model.Log = new MonitoringService().AllLogs.FirstOrDefault(x => x.Id == Model.Id);
            if (Model.Log?.TraceIdentifier != null)
                Model.Request = new MonitoringService().RequestsEnded.FirstOrDefault(x => x.TraceIdentifier == Model.Log.TraceIdentifier);
            return await View();
        }
    }

    public class LogModel
    {
        public string Id;
        public LogEntry Log;
        public RequestResponseInfo Request;
    }
}
