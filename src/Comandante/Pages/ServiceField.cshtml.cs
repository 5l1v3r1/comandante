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
    public class ServiceField : EmbeddedViewModel
    {
        public ServiceFielddModel Model { get; set; }
        private ServicesService _servicesService = new ServicesService();

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServiceFielddModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var fieldName = this.HttpContext.Request.Query["_f"].ToString();
            Model.Service = _servicesService.GetService(serviceName);
            Model.Field = Model.Service.Fields.FirstOrDefault(x => x.Id == fieldName);

            if (IsSubmit())
            {
                var result = _servicesService.InvokeField(Model.Service, Model.Field);
                Model.Errors = result.Errors;
                Model.ExecutionResult = result.Result;
            }

            return await View();
        }
    }

    public class ServiceFielddModel
    {
        public ServiceInfo Service;
        public ServiceFieldInfo Field;
        public string ExecutionResult;
        public List<string> Errors;
        public List<string> Warnings;

        
    }
}
