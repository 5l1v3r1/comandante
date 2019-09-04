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

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new ServiceMethodModel();

            var serviceName = this.HttpContext.Request.Query["_s"].ToString();
            var methodName = this.HttpContext.Request.Query["_m"].ToString();
            Model.Service = new ServicesService().GetService(serviceName);
            Model.Method = Model.Service.Methods.FirstOrDefault(x => x.Id == methodName);

            var fieldsValues = new Dictionary<string, string>();
            var isSubmit = false;
            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.CurrentCultureIgnoreCase))
            {
                fieldsValues = this.HttpContext.Request.Form
                    .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                    .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()?.ToString());
                isSubmit = this.HttpContext.Request.Form.Any(x => x.Key == "_submit");

            }
            Model.FieldsWithValues = Model.Method.Method.GetParameters().Select(x => (x, fieldsValues.FirstOrDefault(v => v.Key == x.Name).Value)).ToList();

            if (isSubmit)
            {
                List<string> errors = new List<string>();
                List<object> methodParametersValus = new List<object>();


                foreach (var param in Model.FieldsWithValues)
                {
                    try
                    {
                        methodParametersValus.Add(Binder.ConvertToType(param.Value, param.Field.ParameterType));
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Cannot set entity property {param.Field.Name} to {param.Value}. Error: {ex.GetAllMessages()}");
                    }
                }
                if (errors.Count > 0)
                {
                    Model.Errors = errors;
                    return await View();
                }


                object result = null;
                try
                {
                    result = Model.Method.Method.Invoke(Model.Service.Service, methodParametersValus.ToArray());
                    if (result is Task)
                    {
                        ((Task)result).Wait();
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Method invocation resulted with the error: {ex.GetAllMessages()}");
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
                } catch(Exception ex)
                {
                    Model.ExecutionResult = ex.GetAllMessages();
                }
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
