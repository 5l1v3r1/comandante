using Comandante.Pages;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comandante.Services
{
    public class ServicesService
    {
        public ServicesService()
        {
        }

        public List<ServiceInfo> GetServices()
        {
            var serviceCollection = ComandanteServiceCollectionExtensions.Services;
            return serviceCollection.Select(x =>
                new ServiceInfo
                {
                    ServiceFullName = x.ServiceType.FullName,
                    ServiceFriendlyName = x.ServiceType.GetFriendlyName(),
                    ServiceType = x.ServiceType,
                    Lifetime = x.Lifetime.ToString(),
                    ImplementationType = x.ImplementationType?.GetFriendlyName()
                }).ToList();
        }

        public ServiceInfo GetService(string fullName)
        {
            var serviceCollection = ComandanteServiceCollectionExtensions.Services;
            var serviceModel = serviceCollection.Select(x =>
                new ServiceInfo
                {
                    ServiceFullName = x.ServiceType.FullName,
                    ServiceFriendlyName = x.ServiceType.GetFriendlyName(),
                    ServiceType = x.ServiceType,
                    Lifetime = x.Lifetime.ToString(),
                    ImplementationType = x.ImplementationType?.GetFriendlyName()
                }).FirstOrDefault(x => x.ServiceFullName == fullName);

            object service = new HttpContextHelper().HttpContext.RequestServices.GetService(serviceModel.ServiceType);
            foreach (var prop in service.GetType().GetProperties())
                serviceModel.Properties.TryAdd(prop.Name, prop.GetValue(service));
            foreach (var field in service.GetType().GetFields())
                serviceModel.Fields.TryAdd(field.Name, field.GetValue(service));
            foreach (var method in service.GetType().GetMethods())
            {
                var methodParameter = method.GetParameters();
                string methodFriendlyName = method.ReturnType + " " + method.Name;
                methodFriendlyName += "(";
                for(int i = 0; i < methodParameter.Length; i++)
                {
                    if (i > 0)
                        methodFriendlyName += ", ";
                    methodFriendlyName += methodParameter[i].ParameterType.GetFriendlyName() + " " + methodParameter[i].Name;
                }
                methodFriendlyName += ")";
                serviceModel.Methods.Add(methodFriendlyName);
            }
            return serviceModel;
        }
    }

    public class ServiceInfo
    {
        public string ServiceFullName;
        public string ServiceFriendlyName;
        public Type ServiceType;
        public string Lifetime;
        public string ImplementationType;

        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public Dictionary<string, object> Fields = new Dictionary<string, object>();
        public List<string> Methods = new List<string>();
    }
}
