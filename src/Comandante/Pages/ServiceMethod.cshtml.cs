using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Comandante.PagesRenderer;
using Comandante.Services;
using Newtonsoft.Json;
using Binder = Comandante.Services.Binder;

namespace Comandante.Pages
{
    public class ServiceMethod : EmbeddedViewModel
    {
        public ServiceMethodModel Model { get; set; }
        private ServicesService _servicesService = new ServicesService();

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServiceMethodModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var methodName = this.HttpContext.Request.Query["_m"].ToString();
            Model.Service = _servicesService.GetService(serviceName);
            Model.Method = Model.Service.Methods.FirstOrDefault(x => x.Id == methodName);

            var fieldsValues = new Dictionary<string, string>();
            if (IsPost())
            {
                fieldsValues = this.HttpContext.Request.Form
                    .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                    .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()?.ToString());

            }
            Model.FieldsWithValues = Model.Method.Method.GetParameters().Select(x => (x, fieldsValues.FirstOrDefault(v => v.Key == x.Name).Value)).ToList();

            if (IsSubmit())
            {
                var result = _servicesService.InvokeMethod(Model.Service, Model.Method, fieldsValues);
                Model.Errors = result.Errors;
                Model.ExecutionResult = result.Result;
            }
            
            return await View();
        }
    }

    public class ServiceMethodModel
    {
        public List<(ParameterInfo Field, string Value)> FieldsWithValues;
        public ServiceInfo Service;
        public ServiceMethodInfo Method;
        public string ExecutionResult;
        public List<string> Errors;
        public List<string> Warnings;
    }
}
