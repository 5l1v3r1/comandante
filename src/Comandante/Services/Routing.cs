﻿using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comandante.Services
{
    public class RoutingService
    {
        public RoutingService()
        {
        }

        public List<RouteInfo> GetRoutes()
        {
            var httpContext = new HttpContextHelper().HttpContext;
            var actionDescriptorCollectionProvider = httpContext.RequestServices.GetService(typeof(IActionDescriptorCollectionProvider)) as IActionDescriptorCollectionProvider;
            if (actionDescriptorCollectionProvider == null)
                return null;

            return actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .Select(x => new RouteInfo
                    {
                        Action = x.RouteValues["Action"],
                        Controller = x.RouteValues["Controller"],
                        RouteValues = JsonConvert.SerializeObject(x.RouteValues),
                        Parameters = x.Parameters != null ? string.Join(", ", x.Parameters.Select(p => p.ParameterType.GetFriendlyName() + " " + p.Name)) : "",
                        Name = x.AttributeRouteInfo?.Name,
                        Template = x.AttributeRouteInfo?.Template,
                        Constraint = x.ActionConstraints == null ? "" : JsonConvert.SerializeObject(x.ActionConstraints),
                        
                    })
                .OrderBy(r => r.Template)
                .ToList();
        }
    }

    public class RouteInfo
    {
        public string Template { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Constraint { get; set; }
        public string RouteValues { get; set; }
        public string Parameters { get; set; }
    }
}
