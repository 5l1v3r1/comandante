using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                    ServiceFullName = x.ServiceType.GetFullFriendlyName(),
                    ServiceFriendlyName = x.ServiceType.GetFriendlyName(),
                    ServiceType = x.ServiceType,
                    Lifetime = x.Lifetime.ToString(),
                    ImplementationType = x.ImplementationType,
                    ImplementationFriendlyName = x.ImplementationType?.GetFriendlyName(),
                    ImplementationFullName = x.ImplementationType?.GetFullFriendlyName()
                }).OrderBy(x => x.ServiceType.Namespace != null && (x.ServiceType.Namespace.StartsWith("Microsoft.") || x.ServiceType.Namespace.StartsWith("System.")) ? 1 : 0).ToList();
        }

        public ServiceInfo GetService(string fullName)
        {
            var serviceModel = GetServices().FirstOrDefault(x => x.ServiceFullName == fullName);
            Type serviceType = serviceModel.ServiceType;
            try
            {
                serviceType = new HttpContextHelper().HttpContext.RequestServices.GetService(serviceType).GetType();
            }
            catch { }
            
            foreach (var prop in serviceType.GetProperties())
            {
                serviceModel.Properties.Add(new ServicePropertyInfo
                {
                    Id = prop.Name,
                    Name = prop.Name,
                    PropertyType = prop.PropertyType.GetFriendlyName(),
                    Poperty = prop
                });
            }

            foreach (var field in serviceType.GetFields())
            {
                serviceModel.Fields.Add(new ServiceFieldInfo
                {
                    Id = field.Name,
                    Name = field.Name,
                    FieldType = field.FieldType.GetFriendlyName(),
                    Field = field
                });
            }

            foreach (var method in serviceType.GetMethods())
            {
                var methodParameter = method.GetParameters();
                string methodParameters = "(";
                for (int i = 0; i < methodParameter.Length; i++)
                {
                    if (i > 0)
                        methodParameters += ", ";
                    methodParameters += methodParameter[i].ParameterType.GetFriendlyName() + " " + methodParameter[i].Name;
                }
                methodParameters += ")";
                string name = method.ReturnType + " " + method.Name + " " + methodParameters;
                serviceModel.Methods.Add(new ServiceMethodInfo
                {
                    Id = name,
                    MethodName = method.Name,
                    MethodReturnType = method.ReturnType.GetFriendlyName(),
                    MethodParameters = methodParameters,
                    Method = method
                });
            }

            return serviceModel;
        }

        public ServiceInvokeResult InvokeMethod(ServiceInfo service, ServiceMethodInfo method, Dictionary<string, string> paramters)
        {
            List<(ParameterInfo Field, string Value)> methodParameters = method.Method.GetParameters().Select(x => (x, paramters.FirstOrDefault(v => v.Key == x.Name).Value)).ToList();
            List<string> errors = new List<string>();
            List<object> invokeValues = new List<object>();

            object serviceInstance = null;
            try
            {
                serviceInstance = new HttpContextHelper().HttpContext.RequestServices.GetService(service.ServiceType);
            }
            catch (Exception ex)
            {
                errors.Add($"Cannot instantiate the service. Error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }


            foreach (var param in methodParameters)
            {
                try
                {
                    invokeValues.Add(Binder.ConvertToType(param.Value, param.Field.ParameterType));
                }
                catch (Exception ex)
                {
                    errors.Add($"Cannot set entity property {param.Field.Name} to {param.Value}. Error: {ex.GetAllMessages()}");
                }
            }
            if (errors.Count > 0)
                return new ServiceInvokeResult { Errors = errors };


            object result = null;
            try
            {
                result = method.Method.Invoke(serviceInstance, invokeValues.ToArray());
                if (result is Task)
                    ((Task)result).Wait();
            }
            catch (Exception ex)
            {
                errors.Add($"Method invocation resulted with the error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }


            try
            {
                if (result == null)
                    return new ServiceInvokeResult { Result = "null" };
                return new ServiceInvokeResult { Result = result.ToJson() };
            }
            catch (Exception ex)
            {
                return new ServiceInvokeResult { Result = ex.GetAllMessages() }
;
            }
        }

        public ServiceInvokeResult InvokeProperty(ServiceInfo service, ServicePropertyInfo property)
        {
            List<string> errors = new List<string>();


            object serviceInstance = null;
            try
            {
                serviceInstance = new HttpContextHelper().HttpContext.RequestServices.GetService(service.ServiceType);
            }
            catch (Exception ex)
            {
                errors.Add($"Cannot instantiate the service. Error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }

            object result;
            try
            {
                result = property.Poperty.GetValue(serviceInstance);
                if (result is Task)
                    ((Task)result).Wait();
            }
            catch (Exception ex)
            {
                errors.Add($"Property invocation resulted with the error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }

            try
            {
                if (result == null)
                    return new ServiceInvokeResult { Result = "null" };
                return new ServiceInvokeResult { Result = result.ToJson()};
            }
            catch (Exception ex)
            {
                return new ServiceInvokeResult { Result = ex.GetAllMessages() };
            }
        }

        public ServiceInvokeResult InvokeField(ServiceInfo service, ServiceFieldInfo field)
        {
            List<string> errors = new List<string>();

            object serviceInstance = null;
            try
            {
                serviceInstance = new HttpContextHelper().HttpContext.RequestServices.GetService(service.ServiceType);
            }
            catch (Exception ex)
            {
                errors.Add($"Cannot instantiate the service. Error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }

            object result;
            try
            {
                result =field.Field.GetValue(serviceInstance);
                if (result is Task)
                    ((Task)result).Wait();
            }
            catch (Exception ex)
            {
                errors.Add($"Field invocation resulted with the error: {ex.GetAllMessages()}");
                return new ServiceInvokeResult { Errors = errors };
            }

            try
            {
                if (result == null)
                    return new ServiceInvokeResult { Result = "null" };
                return new ServiceInvokeResult { Result = result.ToJson() };
            }
            catch (Exception ex)
            {
                return new ServiceInvokeResult { Result = ex.GetAllMessages() };
            }
        }
    }

    public class ServiceInfo
    {
        public string ServiceFullName;
        public string ServiceFriendlyName;
        public Type ServiceType;
        public string Lifetime;
        public string ImplementationFullName;
        public string ImplementationFriendlyName;
        public Type ImplementationType;

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

    public class ServiceInvokeResult
    {
        public List<string> Errors;
        public bool IsSuccess => Errors == null || Errors.Count == 1;
        public string Result;
    }
    
}
