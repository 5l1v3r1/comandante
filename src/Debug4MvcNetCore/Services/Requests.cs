using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Debug4MvcNetCore
{
    public class RequestsService
    {
        private static Dictionary<string, RequestInfo> _requests = new Dictionary<string, RequestInfo>();
        public IEnumerable<RequestInfo> Requests
        {
            get { return _requests.Select(x => x.Value).OrderByDescending(x => x.Created); }
        }

        private static RequestLevel _requestLevel = Debug4MvcNetCore.RequestLevel.OnlyIfError;
        public RequestLevel RequestLevel
        {
            get { return _requestLevel; }
            set { _requestLevel = value; }
        }

        public void AddRequest(HttpContext httpContext)
        {
            if (_requests.ContainsKey(httpContext.TraceIdentifier) == false)
                _requests.Add(httpContext.TraceIdentifier, Create(httpContext));

            //Keep only 2000 requests
            _requests = _requests.Take(2000).ToDictionary(x => x.Key, x => x.Value);
        }

        public RequestInfo Create(HttpContext httpContext)
        {
            var now = DateTime.Now;
            var nowUtc = now.ToUniversalTime();
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.Created = now;
            requestInfo.TraceIdentifier = httpContext.TraceIdentifier;
            requestInfo.RequestQueryString = httpContext.Request.QueryString.Value;
            requestInfo.RequestHost = httpContext.Request.Host.ToString();
            requestInfo.RequestMethod = httpContext.Request.Method;
            requestInfo.RequestScheme = httpContext.Request.Scheme;
            requestInfo.RequestProtocol = httpContext.Request.Protocol;
            requestInfo.RequestPath = httpContext.Request.Path;
            requestInfo.RequestPathBase = httpContext.Request.PathBase;
            
            try
            {
                requestInfo.RequestForm = httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }
            catch { }
            requestInfo.RequestHeaders = httpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.Select(v => v).ToArray());
            requestInfo.RequestCookies = httpContext.Request.Cookies.ToDictionary(x => x.Key, x => x.Value.ToString());
            requestInfo.DateTime.Now = now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            requestInfo.DateTime.NowUtc = nowUtc.ToString("yyyy-MM-ddTHH:mm:sszzz");
            requestInfo.LocalTimeZone.Id = TimeZoneInfo.Local.Id;
            requestInfo.LocalTimeZone.DisplayName = TimeZoneInfo.Local.DisplayName;
            requestInfo.LocalTimeZone.StandardName = TimeZoneInfo.Local.StandardName;
            requestInfo.LocalTimeZone.BaseUtcOffset = TimeZoneInfo.Local.BaseUtcOffset.ToString();
            requestInfo.LocalTimeZone.NowUtcOffset = TimeZoneInfo.Local.GetUtcOffset(now).ToString();
            requestInfo.LocalTimeZone.SupportsDaylightSavingTime = TimeZoneInfo.Local.SupportsDaylightSavingTime;
            requestInfo.UtcTimeZone.Id = TimeZoneInfo.Utc.Id;
            requestInfo.UtcTimeZone.DisplayName = TimeZoneInfo.Utc.DisplayName;
            requestInfo.UtcTimeZone.StandardName = TimeZoneInfo.Utc.StandardName;
            requestInfo.UtcTimeZone.BaseUtcOffset = TimeZoneInfo.Utc.BaseUtcOffset.ToString();
            requestInfo.UtcTimeZone.NowUtcOffset = TimeZoneInfo.Utc.GetUtcOffset(now).ToString();
            requestInfo.UtcTimeZone.SupportsDaylightSavingTime = TimeZoneInfo.Utc.SupportsDaylightSavingTime;
            requestInfo.CurrentCulture.Name = Thread.CurrentThread.CurrentCulture?.Name;
            requestInfo.CurrentCulture.DisplayName = Thread.CurrentThread.CurrentCulture?.DisplayName;
            requestInfo.CurrentCulture.EnglishName = Thread.CurrentThread.CurrentCulture?.EnglishName;
            requestInfo.CurrentCulture.DateTimeFormat = Thread.CurrentThread.CurrentCulture?.DateTimeFormat?.ShortDatePattern + " " + Thread.CurrentThread.CurrentCulture?.DateTimeFormat?.ShortTimePattern;
            requestInfo.CurrentUICulture.Name = Thread.CurrentThread.CurrentUICulture?.Name;
            requestInfo.CurrentUICulture.DisplayName = Thread.CurrentThread.CurrentUICulture?.DisplayName;
            requestInfo.CurrentUICulture.EnglishName = Thread.CurrentThread.CurrentUICulture?.EnglishName;
            requestInfo.CurrentUICulture.DateTimeFormat = Thread.CurrentThread.CurrentUICulture?.DateTimeFormat?.ShortDatePattern + " " + Thread.CurrentThread.CurrentUICulture?.DateTimeFormat?.ShortTimePattern;
            requestInfo.Connection.Id = httpContext.Connection.Id;
            requestInfo.Connection.LocalIpAddress = httpContext.Connection.LocalIpAddress?.ToString();
            requestInfo.Connection.LocalPort = httpContext.Connection.LocalPort;
            requestInfo.Connection.RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            requestInfo.Connection.RemotePort = httpContext.Connection.RemotePort;
            requestInfo.Connection.ClientCertificate = httpContext.Connection.ClientCertificate?.ToString();
            requestInfo.Identities =
                httpContext
                .User
                .Identities
                .Select(i =>
                    new IdentityDebugInfo
                    {
                        Name = i.Name,
                        AuthenticationType = i.AuthenticationType,
                        IsAuthenticated = i.IsAuthenticated,
                        Label = i.Label,
                        Actor = i.Name,
                        NameClaimType = i.NameClaimType,
                        RoleClaimType = i.RoleClaimType,
                        Claims = i.Claims.Select(c =>
                            new IdentityClaimDebugInfo
                            {
                                Issuer = c.Issuer,
                                OriginalIssuer = c.OriginalIssuer,
                                Subject = c.Subject?.ToString(),
                                Type = c.Type,
                                Value = c.Value,
                                ValueType = c.ValueType,
                                Properties = c.Properties.ToDictionary(p => p.Key, p => p.Value),
                            }).ToArray()
                    })
                .ToArray();

            return requestInfo;
        }

        public void ClearRequests()
        {
            _requests.Clear();
        }

        public void CleanUp(HttpContext context)
        {
            
        }
    }

    public enum RequestLevel : int
    {
        OnlyIfError = 3,
        OnlyMvc = 2,
        Everything = 1,
        Node = 0
    }

    public class RequestInfo
    {
        public string TraceIdentifier;
        public string RequestQueryString;
        public string RequestHost;
        public string RequestMethod;
        public string RequestScheme;
        public string RequestProtocol;
        public string RequestPath;
        public string RequestPathBase;
        public Dictionary<string, string[]> RequestHeaders = new Dictionary<string, string[]>();
        public Dictionary<string, string> RequestCookies = new Dictionary<string, string>();
        public Dictionary<string, string[]> RequestForm = new Dictionary<string, string[]>();
        public DateTimeDebugInfo DateTime = new DateTimeDebugInfo();
        public ConnectionDebugInfo Connection = new ConnectionDebugInfo();
        public IdentityDebugInfo[] Identities = new IdentityDebugInfo[0];
        public IdentityDebugInfo Identity => Identities.FirstOrDefault();

        public DateTime Created { get; internal set; }

        public TimeZoneDebugInfo LocalTimeZone = new TimeZoneDebugInfo();
        public TimeZoneDebugInfo UtcTimeZone = new TimeZoneDebugInfo();
        public CultureDebugInfo CurrentCulture = new CultureDebugInfo();
        public CultureDebugInfo CurrentUICulture = new CultureDebugInfo();
        
    }

    public class DateTimeDebugInfo
    {
        public string Now;
        public string NowUtc;
    }

    public class TimeZoneDebugInfo
    {
        public string Id;
        public string DisplayName;
        public string StandardName;
        public string NowUtcOffset;
        public string BaseUtcOffset;
        public bool SupportsDaylightSavingTime;
    }

    public class CultureDebugInfo
    {
        public string Name;
        public string DisplayName;
        public string EnglishName;
        public string DateTimeFormat;
    }

    public class IdentityDebugInfo
    {
        public string Name;
        public string AuthenticationType;
        public bool IsAuthenticated;
        public string Label;
        public string Actor;
        public string NameClaimType;
        public string RoleClaimType;
        public IdentityClaimDebugInfo[] Claims = new IdentityClaimDebugInfo[0];
    }

    public class IdentityClaimDebugInfo
    {
        public string Subject;
        public string Type;
        public string Value;
        public string ValueType;
        public string Issuer;
        public string OriginalIssuer;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }

    public class ConnectionDebugInfo
    {
        public string Id;
        public string LocalIpAddress;
        public int LocalPort;
        public string RemoteIpAddress;
        public int RemotePort;
        public string ClientCertificate;
    }
}
