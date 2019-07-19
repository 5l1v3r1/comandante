using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Logs : EmbeddedPageModel
    {
        public LogsModel Model { get; set; }

        public override async Task InitPage()
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
