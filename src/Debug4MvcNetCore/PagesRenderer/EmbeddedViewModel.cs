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

namespace Debug4MvcNetCore.PagesRenderer
{
    public abstract class EmbeddedViewModel
    {
        public HttpContext HttpContext { get; set; }
        public string ViewName { get; set; }

        public abstract Task<EmbededViewResult> InitView();


        private string AttributeEnding { get; set; }
        private List<string> AttributeValues { get; set; }

        public void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            HttpContext?.Response.WriteAsync(begining);
            AttributeEnding = ending;
        }

        public void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral)
        {
            if (value != null && AttributeValues == null)
                AttributeValues = new List<string>();

            if (value != null)
                AttributeValues.Add(value.ToString());
        }

        public void EndWriteAttribute()
        {
            if (AttributeValues != null)
            {
                var attributes = string.Join(" ", AttributeValues);
                HttpContext?.Response.WriteAsync(attributes);
            }

            AttributeValues = null;

            HttpContext?.Response.WriteAsync(AttributeEnding);
            AttributeEnding = null;
        }

        public void WriteLiteral(string literal)
        {
            HttpContext?.Response.WriteAsync(literal);
        }

        public void Write(object obj)
        {
            if (obj != null)
                HttpContext?.Response.WriteAsync(obj.ToString());
        }

        private Stack<TextWriter> TextWriters { get; set; } = new Stack<TextWriter>();

        public void PushWriter(TextWriter writer)
        {
            TextWriters.Push(writer);
        }

        public TextWriter PopWriter()
        {
            return TextWriters.Pop();
        }

        public object RenderResource(string resource)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (resource.EndsWith(".js"))
            {
                using (Stream resourceStream = assembly.GetManifestResourceStream("Debug4MvcNetCore.wwwroot." + resource))
                {
                    string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                    return "<script>" + resourceContent + "</script>";
                }
            }
            if (resource.EndsWith(".css"))
            {
                using (Stream resourceStream = assembly.GetManifestResourceStream("Debug4MvcNetCore.wwwroot." + resource))
                {
                    string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                    return "<style>" + resourceContent + "</style>";
                }
            }
            return "";
        }

        public async Task RenderPartialView(string viewName, object model)
        {
            await new EmbeddedViewRenderer().RenderView(viewName, this.HttpContext, model);
        }

        public async virtual Task ExecuteAsync()
        {
            await Task.Yield();
        }

        public async Task<EmbededViewResult> View()
        {
            return await Task.FromResult(new EmbededViewViewResult());
        }

        public async Task<EmbededViewResult> Json(string json)
        {
            return await Task.FromResult(new EmbededViewJsonResult(json));
        }
    }

    public class EmbededViewResult
    {

    }

    public class EmbededViewViewResult : EmbededViewResult
    {

    }

    public class EmbededViewJsonResult : EmbededViewResult
    {
        public string Json { get; }

        public EmbededViewJsonResult(string json)
        {
            Json = json;
        }
    }

    public class EmbededViewRedirectResult : EmbededViewResult
    {
        public string Url { get; }

        public EmbededViewRedirectResult(string url)
        {
            Url = url;
        }
    }
}
