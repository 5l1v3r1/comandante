using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Threading;
using Debug4MvcNetCore.PagesRenderer;

namespace Debug4MvcNetCore
{
    public class Debug4MvcNetCoreLoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public Debug4MvcNetCoreLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            new HttpContextService().HttpContext = context;
            new RequestsService().AddRequest(context);

            var request = context.Request;
            var response = context.Response;
            
            var matchSubPage  = Regex.Match(request.Path, "/debug/([A-Za-z0-9]*).*");
            if (matchSubPage != null && matchSubPage.Success && matchSubPage.Index == 0 && matchSubPage.Groups.Count == 2 && matchSubPage.Groups[1].Success)
            {
                var renderer = new EmbeddedViewRenderer();
                await renderer.RenderView(matchSubPage.Groups[1].Value, context);
                return;
            }

            var matchIndexPage = Regex.Match(request.Path, "/debug[\\?#/$]*");
            if (matchIndexPage != null && matchIndexPage.Success && matchIndexPage.Index == 0)
            {
                var renderer = new EmbeddedViewRenderer();
                await renderer.RenderView("Index", context);
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);

            var resonse = context.Response;
        }
    }

    public static class Debug4MvcNetCoreLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseDebug4MvcNetCore(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            var loggerFactory = (ILoggerFactory)builder.ApplicationServices.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(new Debug4MvcNetCoreLoggerProvider());

            return builder.UseMiddleware<Debug4MvcNetCoreLoggerMiddleware>();
        }
    }



}
