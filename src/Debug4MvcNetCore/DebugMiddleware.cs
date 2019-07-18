using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using System.Text.RegularExpressions;

namespace Debug4MvcNetCore
{
    public class DebugMiddleware
    {
        private readonly RequestDelegate _next;

        public DebugMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            var matchSubPage  = Regex.Match(request.Path, "/debug/([A-Za-z0-9]*).*");
            if (matchSubPage != null && matchSubPage.Success && matchSubPage.Index == 0 && matchSubPage.Groups.Count == 2 && matchSubPage.Groups[1].Success)
            {
                var renderer = new EmbeddedPageRenderer();
                await renderer.Render(matchSubPage.Groups[1].Value, context);
                return;
            }

            var matchIndexPage = Regex.Match(request.Path, "/debug[\\?#/$]*");
            if (matchIndexPage != null && matchIndexPage.Success && matchIndexPage.Index == 0)
            {
                var renderer = new EmbeddedPageRenderer();
                await renderer.Render("Index", context);
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);


            var resonse = context.Response;
        }
    }

    public static class DebugMiddlewareExtensions
    {
        public static IApplicationBuilder UseDebug(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            return builder.UseMiddleware<DebugMiddleware>();
        }
    }
}
