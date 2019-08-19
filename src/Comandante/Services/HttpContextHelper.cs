using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Comandante
{
    public class HttpContextHelper
    {
        private static AsyncLocal<HttpContextHolder> _httpContextCurrent = new AsyncLocal<HttpContextHolder>();

        public HttpContext HttpContext
        {
            get
            {
                return _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _httpContextCurrent.Value = new HttpContextHolder { Context = value };
                }
            }
        }

        private class HttpContextHolder
        {
            public HttpContext Context;
        }

        private static AsyncLocal<RequestResponseInfotHolder> _requestResponseInfoHolder = new AsyncLocal<RequestResponseInfotHolder>();

        public RequestResponseInfo RequestResponseInfo
        {
            get
            {
                return _requestResponseInfoHolder.Value?.Info;
            }
            set
            {
                var holder = _requestResponseInfoHolder.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Info = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _requestResponseInfoHolder.Value = new RequestResponseInfotHolder { Info = value };
                }
            }
        }

        private class RequestResponseInfotHolder
        {
            public RequestResponseInfo Info;
        }


        public (bool IsComandanteRequest, string ViewName) IsComandanteRequest(HttpContext context)
        {
            var matchSubPage = Regex.Match(context.Request.Path, "/debug/([A-Za-z0-9]*).*");
            if (matchSubPage != null && matchSubPage.Success && matchSubPage.Index == 0 && matchSubPage.Groups.Count == 2 && matchSubPage.Groups[1].Success)
                return (true, matchSubPage.Groups[1].Value);

            var matchIndexPage = Regex.Match(context.Request.Path, "/debug[\\?#/$]*");
            if (matchIndexPage != null && matchIndexPage.Success && matchIndexPage.Index == 0)
                return (true, "Index");

            return (false, null);
        }
    }
}
