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
        private RequestsService _requestsService = new RequestsService();

        public LogsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new LogsModel();
            Model.Logs = _requestsService.Logs.Take(1000).ToList();
            await Task.CompletedTask;
        }
    }

    public class LogsModel
    {
        public IEnumerable<LogEntry> Logs { get; set; }

    }

}
