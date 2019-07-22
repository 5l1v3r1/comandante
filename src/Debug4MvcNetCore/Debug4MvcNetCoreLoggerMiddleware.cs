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
        private HttpContextHelper _httpContextHelper = new HttpContextHelper();
        private RequestsService _requestsService = new RequestsService();
        private LogsService _logsService = new LogsService();

        public Debug4MvcNetCoreLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _httpContextHelper.HttpContext = context;

                var isDebugRequest = _httpContextHelper.IsDebug4MvcNetCoreRequest(context);
                if (isDebugRequest.IsDebug4MvcNetCoreRequest)
                {
                    DisableCache(context);
                    var renderer = new EmbeddedViewRenderer();
                    await renderer.RenderView(isDebugRequest.ViewName, context);
                    return;
                }

                _requestsService.AddRequest(context);

                await _next(context);

                _logsService.CleanUp(context);
                _requestsService.CleanUp(context);

            } catch (Exception ex)
            {
                try
                {
                    var logger = (ILogger<Debug4MvcNetCoreLoggerMiddleware>)context.RequestServices.GetService(typeof(ILogger<Debug4MvcNetCoreLoggerMiddleware>));
                    logger.LogError(ex.Message, ex);
                }
                catch { }
            }
        }

        public void DisableCache(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            httpContext.Response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
            httpContext.Response.Headers.Add("Expires", "0"); // Proxies.
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
