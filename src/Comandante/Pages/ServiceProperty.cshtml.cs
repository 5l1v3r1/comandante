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
    public class ServiceProperty : EmbeddedViewModel
    {
        public ServicePropertydModel Model { get; set; }
        private ServicesService _servicesService = new ServicesService();

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServicePropertydModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var propertyName = this.HttpContext.Request.Query["_p"].ToString();
            Model.Service = _servicesService.GetService(serviceName);
            Model.Property = Model.Service.Properties.FirstOrDefault(x => x.Id == propertyName);

            if (this.IsSubmit())
            {
                var result = _servicesService.InvokeProperty(Model.Service, Model.Property);
                Model.Errors = result.Errors;
                Model.ExecutionResult = result.Result;
            }

            return await View();
        }
    }

    public class ServicePropertydModel
    {
        public ServiceInfo Service;
        public ServicePropertyInfo Property;
        public string ExecutionResult;
        public List<string> Errors;
        public List<string> Warnings;
    }
}
