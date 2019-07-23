using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Debug4MvcNetCore.Pages
{
    public class Index : EmbeddedViewModel
    {
        public IndexModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new IndexModel();
            Model.Request = new RequestsService().CreateRequestResponseInfo(this.HttpContext);
        }
    }

    public class IndexModel
    {
        public Debug4MvcNetCore.RequestResponseInfo Request = new Debug4MvcNetCore.RequestResponseInfo();
    }

}
