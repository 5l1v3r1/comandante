using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Debug4MvcNetCore
{
    public abstract class EmbeddedPageModel
    {
        public HttpContext HttpContext { get; set; }

        public abstract Task InitPage();

        public void WriteLiteral(string literal)
        {
            HttpContext?.Response.WriteAsync(literal);
        }

        public void Write(object obj)
        {
            if (obj != null)
                HttpContext?.Response.WriteAsync(obj.ToString());
        }

        public object RenderResource(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (resource.EndsWith(".js"))
            {
                Stream resourceStream = assembly.GetManifestResourceStream("Debug4MvcNetCore.wwwroot." + resource);
                string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                return "<script>" + resourceContent + "</script>";
            }
            if (resource.EndsWith(".css"))
            {
                Stream resourceStream = assembly.GetManifestResourceStream("Debug4MvcNetCore.wwwroot." + resource);
                string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                return "<style>" + resourceContent + "</style>";
            }
            return "";
        }

        public async virtual Task ExecuteAsync()
        {
            await Task.Yield();
        }
    }
}
