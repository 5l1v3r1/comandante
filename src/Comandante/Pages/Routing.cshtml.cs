using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Comandante.Pages
{
    public class Routing : EmbeddedViewModel
    {
        public RoutingModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new RoutingModel();
            Model.Routes = new RoutingService().GetRoutes();
            return await View();
        }
    }

    public class RoutingModel
    {
        public List<RouteInfo> Routes { get; set; }
    }

}
