using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Requests : EmbeddedViewModel
    {
        public RequestsModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new RequestsModel();
            Model.Requests = new RequestsService().Requests;
        }
    }

    public class RequestsModel
    {
        public IEnumerable<Debug4MvcNetCore.RequestInfo> Requests { get; internal set; }
    }

}
