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
using Microsoft.Extensions.DependencyInjection;
using Comandante.Services.EventListeners;
using System.IO;
using System.IO.Compression;

namespace Comandante
{
    public class ComandanteMiddleware
    {
        private readonly RequestDelegate _next;
        private HttpContextHelper _httpContextHelper = new HttpContextHelper();
        private MonitoringService _requestsService = new MonitoringService();
        private MonitoringService _logsService = new MonitoringService();

        public ComandanteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _httpContextHelper.HttpContext = context;
                var isComandanteRequest = _httpContextHelper.IsComandanteRequest(context);
                if (isComandanteRequest.IsComandanteRequest)
                {
                    await RenderComandanteView(context, isComandanteRequest.ViewName);
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
            await EnableCompression(context);
            var renderer = new EmbeddedViewRenderer();
            await renderer.RenderView(ViewName, context);
        }

        public void DisableCache(HttpContext context)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            context.Response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
            context.Response.Headers.Add("Expires", "0"); // Proxies.
        }

        public async Task EnableCompression(HttpContext httpContext)
        {
            bool isGZipSupported = false;
            string acceptEncoding = httpContext.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(acceptEncoding) && (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate")))
                isGZipSupported = true;

            if (isGZipSupported)
            {
                if (acceptEncoding.Contains("gzip"))
                {
                    httpContext.Response.Body = new BufferedStream(new GZipStream(
                        httpContext.Response.Body,
                        CompressionMode.Compress));
                    httpContext.Response.Headers.Remove("Content-Encoding");
                    httpContext.Response.Headers.Append("Content-Encoding", "gzip");
                }
                else
                {
                    httpContext.Response.Body = new BufferedStream(new DeflateStream(
                        httpContext.Response.Body,
                        CompressionMode.Compress));
                    httpContext.Response.Headers.Remove("Content-Encoding");
                    httpContext.Response.Headers.Append("Content-Encoding", "deflate");
                }
            }
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
        static internal string BaseUrl = "/comandante";

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

    public static class ComandanteServiceCollectionExtensions
    {
        static internal IServiceCollection Services;
        //static internal DotNETRuntimeEventListener GcFinalizersEventListener;
        public static void AddComandante(this IServiceCollection services)
        {
            Services = services;
            //GcFinalizersEventListener = new DotNETRuntimeEventListener();
        }
    }


    
}
