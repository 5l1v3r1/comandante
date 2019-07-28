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
    public class Log : EmbeddedViewModel
    {
        private RequestsService _requestsService = new RequestsService();

        public LogModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new LogModel();
            if (this.HttpContext.Request.Query.ContainsKey("Id"))
                Model.Id = this.HttpContext.Request.Query["Id"].ToString().Trim();
            Model.Log = new RequestsService().AllLogs.FirstOrDefault(x => x.Id == Model.Id);
            if (Model.Log?.TraceIdentifier != null)
                Model.Request = new RequestsService().RequestsEnded.FirstOrDefault(x => x.TraceIdentifier == Model.Log.TraceIdentifier);
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
