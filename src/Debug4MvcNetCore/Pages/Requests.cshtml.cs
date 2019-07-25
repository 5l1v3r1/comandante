using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Debug4MvcNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Requests : EmbeddedViewModel
    {
        private RequestsService _requestsService = new RequestsService();
        public RequestsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new RequestsModel();
            Model.Requests = _requestsService.Requests;
            await Task.CompletedTask;
        }
    }

    public class RequestsModel
    {
        public IEnumerable<Debug4MvcNetCore.RequestResponseInfo> Requests { get; set; }
    }

}
