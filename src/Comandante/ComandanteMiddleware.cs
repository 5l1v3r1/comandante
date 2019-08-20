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
using Comandante.PagesRenderer;
using Comandante.Services;

namespace Comandante
{
    public class ComandanteMiddleware
    {
        private readonly RequestDelegate _next;
        private HttpContextHelper _httpContextHelper = new HttpContextHelper();
        private RequestsService _requestsService = new RequestsService();
        private RequestsService _logsService = new RequestsService();

        public ComandanteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _httpContextHelper.HttpContext = context;
                var isDebugRequest = _httpContextHelper.IsComandanteRequest(context);
                if (isDebugRequest.IsComandanteRequest)
                {
                    await RenderComandanteView(context, isDebugRequest.ViewName);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogException(context, ex);
            }

            try
            {
                StartRequest(context);
                await _next(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex);
                throw;
            }
            finally
            {
                EndRequest(context);
            }
        }

        public void StartRequest(HttpContext context)
        {
            try
            {
                _requestsService.StartRequest(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex);
            }
        }

        public void EndRequest(HttpContext context)
        {
            try
            {
                _requestsService.EndRequest(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex);
            }
        }

        public async Task RenderComandanteView(HttpContext context, string ViewName)
        {
            DisableCache(context);
            var renderer = new EmbeddedViewRenderer();
            await renderer.RenderView(ViewName, context);
        }

        public void DisableCache(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            httpContext.Response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
            httpContext.Response.Headers.Add("Expires", "0"); // Proxies.
        }
        
        public void LogException(HttpContext context, Exception ex)
        {
            try
            {
                var logger = (ILogger<ComandanteMiddleware>)context.RequestServices.GetService(typeof(ILogger<ComandanteMiddleware>));
                logger.LogError(ex.Message, ex);
            }
            catch { }
        }
    }

    public static class ComandanteMiddlewareExtensions
    {
        public static IApplicationBuilder UseComandante(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            var loggerFactory = (ILoggerFactory)builder.ApplicationServices.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(new ComandanteLoggerProvider());

            new EntityFrameworkService().EnableSensitiveDataLogging(builder.ApplicationServices);

            return builder.UseMiddleware<ComandanteMiddleware>();
        }
    }



}
