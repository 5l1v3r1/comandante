using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Comandante.PagesRenderer
{
    public abstract class EmbeddedViewModel
    {
        public HttpContext HttpContext { get; set; }
        public string ViewName { get; set; }
        public Html Html { get; set; }

        public abstract Task<EmbededViewResult> InitView();


        private string AttributeEnding { get; set; }
        private List<string> AttributeValues { get; set; }

        public void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
        {
            HttpContext.Response.WriteAsync(begining);
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
                HttpContext.Response.WriteAsync(attributes);
            }

            AttributeValues = null;

            HttpContext.Response.WriteAsync(AttributeEnding);
            AttributeEnding = null;
        }

        public void WriteLiteral(string literal)
        {
            HttpContext.Response.WriteAsync(literal);
        }

        public void Write(object obj)
        {
            if (obj != null)
                WriteEncodedText(obj.ToString());
        }

        public void WriteEncodedText(string text)
        {
            if (text != null)
                HttpContext.Response.WriteAsync(System.Net.WebUtility.HtmlEncode(text));
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
                using (Stream resourceStream = assembly.GetManifestResourceStream("Comandante.wwwroot." + resource))
                {
                    string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                    HttpContext.Response.WriteAsync("<script>" + resourceContent + "</script>");
                }
            }
            if (resource.EndsWith(".css"))
            {
                using (Stream resourceStream = assembly.GetManifestResourceStream("Comandante.wwwroot." + resource))
                {
                    string resourceContent = new StreamReader(resourceStream).ReadToEnd();
                    HttpContext.Response.WriteAsync("<style>" + resourceContent + "</style>");
                }
            }
            return "";
        }

        public object RenderPartialView(string viewName, object model)
        {
            new EmbeddedViewRenderer().RenderView(viewName, this.HttpContext, model).Wait();
            return null;
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

    public class Html : IHtmlHelper
    {
        public HttpContext HttpContext { get; }
        public Html5DateRenderingMode Html5DateRenderingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string IdAttributeDotReplacement => throw new NotImplementedException();

        public IModelMetadataProvider MetadataProvider => throw new NotImplementedException();

        public dynamic ViewBag => throw new NotImplementedException();

        public ViewContext ViewContext => throw new NotImplementedException();

        public ViewDataDictionary ViewData => throw new NotImplementedException();

        public ITempDataDictionary TempData => throw new NotImplementedException();

        public UrlEncoder UrlEncoder => throw new NotImplementedException();

        public Html(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public IHtmlContent Partial(string viewName)
        {
            new EmbeddedViewRenderer().RenderView(viewName, this.HttpContext).Wait();
            return null;
        }

        public IHtmlContent Partial(string viewName, object model)
        {
            new EmbeddedViewRenderer().RenderView(viewName, this.HttpContext, model).Wait();
            return null;
        }

        public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent AntiForgeryToken()
        {
            throw new NotImplementedException();
        }

        public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public MvcForm BeginRouteForm(string routeName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent CheckBox(string expression, bool? isChecked, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            throw new NotImplementedException();
        }

        public string DisplayName(string expression)
        {
            throw new NotImplementedException();
        }

        public string DisplayText(string expression)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent DropDownList(string expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData)
        {
            throw new NotImplementedException();
        }

        public string Encode(object value)
        {
            throw new NotImplementedException();
        }

        public string Encode(string value)
        {
            throw new NotImplementedException();
        }

        public void EndForm()
        {
            throw new NotImplementedException();
        }

        public string FormatValue(object value, string format)
        {
            throw new NotImplementedException();
        }

        public string GenerateIdFromName(string fullName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SelectListItem> GetEnumSelectList<TEnum>() where TEnum : struct
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SelectListItem> GetEnumSelectList(Type enumType)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Hidden(string expression, object value, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public string Id(string expression)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Label(string expression, string labelText, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent ListBox(string expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public string Name(string expression)
        {
            throw new NotImplementedException();
        }

        public Task<IHtmlContent> PartialAsync(string partialViewName, object model, ViewDataDictionary viewData)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Password(string expression, object value, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent RadioButton(string expression, object value, bool? isChecked, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Raw(string value)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent Raw(object value)
        {
            throw new NotImplementedException();
        }

        public Task RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent TextArea(string expression, string value, int rows, int columns, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent TextBox(string expression, object value, string format, object htmlAttributes)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag)
        {
            throw new NotImplementedException();
        }

        public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag)
        {
            throw new NotImplementedException();
        }

        public string Value(string expression, string format)
        {
            throw new NotImplementedException();
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
