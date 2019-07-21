using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Logs : EmbeddedViewModel
    {
        public LogsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new LogsModel();
            Model.Logs = new LogsService().Logs;
        }
    }

    public class LogsModel
    {
        public IEnumerable<LogEntry> Logs { get; internal set; }
    }

}
