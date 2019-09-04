using Comandante.Pages;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            {
                serviceModel.Properties.Add(new ServicePropertyInfo
                {
                    Id = prop.Name,
                    Name = prop.Name,
                    PropertyType = prop.PropertyType.GetFriendlyName(),
                    Poperty = prop
                });
            }

            foreach (var field in service.GetType().GetFields())
            {
                serviceModel.Fields.Add(new ServiceFieldInfo
                {
                    Id = field.Name,
                    Name = field.Name,
                    FieldType = field.FieldType.GetFriendlyName(),
                    Field = field
                });
            }

            foreach (var method in service.GetType().GetMethods())
            {
                var methodParameter = method.GetParameters();
                string methodParameters = "(";
                for(int i = 0; i < methodParameter.Length; i++)
                {
                    if (i > 0)
                        methodParameters += ", ";
                    methodParameters += methodParameter[i].ParameterType.GetFriendlyName() + " " + methodParameter[i].Name;
                }
                methodParameters += ")";
                string name = method.ReturnType + " " + method.Name + " " + methodParameters;
                serviceModel.Methods.Add(new ServiceMethodInfo {
                    Id = name,
                    MethodName = method.Name,
                    MethodReturnType = method.ReflectedType.GetFriendlyName(),
                    MethodParameters = methodParameters,
                    Method = method });
            }

            serviceModel.Service = service;
            return serviceModel;
        }
    }

    public class ServiceInfo
    {
        public string ServiceFullName;
        public string ServiceFriendlyName;
        public Type ServiceType;
        public object Service;
        public string Lifetime;
        public string ImplementationType;
        
        public List<ServiceFieldInfo> Fields = new List<ServiceFieldInfo>();
        public List<ServicePropertyInfo> Properties = new List<ServicePropertyInfo>();
        public List<ServiceMethodInfo> Methods = new List<ServiceMethodInfo>();
    }

    public class ServiceMethodInfo
    {
        public string Id;
        public string MethodName;
        public string MethodReturnType;
        public string MethodParameters;
        public System.Reflection.MethodInfo Method;
    }

    public class ServicePropertyInfo
    {
        public string Id;
        public string Name;
        public string PropertyType;
        internal PropertyInfo Poperty;
    }

    public class ServiceFieldInfo
    {
        public string Id;
        public string Name;
        public string FieldType;
        internal FieldInfo Field;
    }
    
}
