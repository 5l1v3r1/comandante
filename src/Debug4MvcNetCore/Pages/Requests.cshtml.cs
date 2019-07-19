using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Requests : EmbeddedPageModel
    {
        public RequestsModel Model { get; set; }

        public override async Task InitPage()
        {
            Model = new RequestsModel();
            Model.Requests = new LogsService().Requests;
        }
    }

    public class RequestsModel
    {
        public IEnumerable<DebugInfo> Requests { get; internal set; }
    }

}
