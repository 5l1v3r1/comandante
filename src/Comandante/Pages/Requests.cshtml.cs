using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class Requests : EmbeddedViewModel
    {
        private MonitoringService _requestsService = new MonitoringService();
        public RequestsModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new RequestsModel();
            Model.Requests = _requestsService.AllRequests;
            return await View();
        }
    }

    public class RequestsModel
    {
        public IEnumerable<Comandante.RequestResponseInfo> Requests { get; set; }
    }

}
