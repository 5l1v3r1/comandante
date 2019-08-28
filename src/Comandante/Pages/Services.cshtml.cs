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
using Microsoft.Extensions.DependencyInjection;

namespace Comandante.Pages
{
    public class Services : EmbeddedViewModel
    {
        public ServicesModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new ServicesModel();
            var serviceCollection = ComandanteServiceCollectionExtensions.Services;
            if (serviceCollection != null)
            {
                Model.Services = serviceCollection.Select(x => new ServiceInfo { ServiceType = x.ServiceType.GetFriendlyName(), Lifetime = x.Lifetime.ToString(), ImplementationType = x.ImplementationType?.GetFriendlyName() }).ToList();
            }
            return await View();
        }
    }

    public class ServicesModel
    {
        public List<ServiceInfo> Services { get; set; }
    }

    public class ServiceInfo
    {
        public string ServiceType;
        public string Lifetime;
        public string ImplementationType;
    }
}
