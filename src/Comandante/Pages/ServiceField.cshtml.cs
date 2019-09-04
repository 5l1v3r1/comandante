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

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServiceFielddModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var fieldName = this.HttpContext.Request.Query["_f"].ToString();
            Model.Service = new ServicesService().GetService(serviceName);
            Model.Field = Model.Service.Fields.FirstOrDefault(x => x.Id == fieldName);

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
                    result = Model.Field.Field.GetValue(Model.Service.Service);
                    if (result is Task)
                    {
                        ((Task)result).Wait();
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Field invocation resulted with the error: {ex.GetAllMessages()}");
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

    public class ServiceFielddModel
    {
        public ServiceInfo Service;
        public ServiceFieldInfo Field;
        public string ExecutionResult;
        public List<string> Errors;
        public List<string> Warnings;

        
    }
}
