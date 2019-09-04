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

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServicePropertydModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var propertyName = this.HttpContext.Request.Query["_p"].ToString();
            Model.Service = new ServicesService().GetService(serviceName);
            Model.Property = Model.Service.Properties.FirstOrDefault(x => x.Id == propertyName);

            bool isSubmit = false;
            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.CurrentCultureIgnoreCase))
            {
                isSubmit = this.HttpContext.Request.Form.Any(x => x.Key == "_submit");
            }

            if (isSubmit)
            {
                List<string> errors = new List<string>();
                object result;
                try
                {
                    result = Model.Property.Poperty.GetValue(Model.Service.Service);
                    if (result is Task)
                    {
                        ((Task)result).Wait();
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Property invocation resulted with the error: {ex.GetAllMessages()}");
                    Model.Errors = errors;
                    return await View();
                }

                try
                {
                    if (result == null)
                    {
                        Model.ExecutionResult = "null";
                        return await View();
                    }
                    Model.ExecutionResult = JsonConvert.SerializeObject(result, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    Model.ExecutionResult = ex.GetAllMessages();
                }
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
