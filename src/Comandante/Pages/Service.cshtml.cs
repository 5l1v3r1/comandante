using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Comandante.Pages
{
    public class Service : EmbeddedViewModel
    {
        public ServiceModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServiceModel();
            if (this.HttpContext.Request.Query.ContainsKey("s"))
            {
                var s = this.HttpContext.Request.Query["s"].ToString().Trim();
                Model.Service = new ServicesService().GetService(s);
            }

            return await View();
        }
    }

    public class ServiceModel
    {
        public ServiceInfo Service { get; set; }
    }
}
