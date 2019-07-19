using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Debug4MvcNetCore
{
    public class DebugInfoService
    {
        public DebugInfo Create(HttpContext httpContext)
        {
            var now = DateTime.Now;
            var nowUtc = now.ToUniversalTime();
            DebugInfo debugInfo = new DebugInfo();
            debugInfo.Created = now;
            debugInfo.TraceIdentifier = httpContext.TraceIdentifier;
            debugInfo.RequestQueryString = httpContext.Request.QueryString.Value;
            debugInfo.RequestHost = httpContext.Request.Host.ToString();
            debugInfo.RequestMethod = httpContext.Request.Method;
            debugInfo.RequestScheme = httpContext.Request.Scheme;
            debugInfo.RequestProtocol = httpContext.Request.Protocol;
            debugInfo.RequestPath = httpContext.Request.Path;
            debugInfo.RequestPathBase = httpContext.Request.PathBase;
            
            try
            {
                debugInfo.RequestForm = httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }
            catch { }
            debugInfo.RequestHeaders = httpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.Select(v => v).ToArray());
            debugInfo.RequestCookies = httpContext.Request.Cookies.ToDictionary(x => x.Key, x => x.Value.ToString());
            debugInfo.DateTime.Now = now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            debugInfo.DateTime.NowUtc = nowUtc.ToString("yyyy-MM-ddTHH:mm:sszzz");
            debugInfo.LocalTimeZone.Id = TimeZoneInfo.Local.Id;
            debugInfo.LocalTimeZone.DisplayName = TimeZoneInfo.Local.DisplayName;
            debugInfo.LocalTimeZone.StandardName = TimeZoneInfo.Local.StandardName;
            debugInfo.LocalTimeZone.BaseUtcOffset = TimeZoneInfo.Local.BaseUtcOffset.ToString();
            debugInfo.LocalTimeZone.NowUtcOffset = TimeZoneInfo.Local.GetUtcOffset(now).ToString();
            debugInfo.LocalTimeZone.SupportsDaylightSavingTime = TimeZoneInfo.Local.SupportsDaylightSavingTime;
            debugInfo.UtcTimeZone.Id = TimeZoneInfo.Utc.Id;
            debugInfo.UtcTimeZone.DisplayName = TimeZoneInfo.Utc.DisplayName;
            debugInfo.UtcTimeZone.StandardName = TimeZoneInfo.Utc.StandardName;
            debugInfo.UtcTimeZone.BaseUtcOffset = TimeZoneInfo.Utc.BaseUtcOffset.ToString();
            debugInfo.UtcTimeZone.NowUtcOffset = TimeZoneInfo.Utc.GetUtcOffset(now).ToString();
            debugInfo.UtcTimeZone.SupportsDaylightSavingTime = TimeZoneInfo.Utc.SupportsDaylightSavingTime;
            debugInfo.CurrentCulture.Name = Thread.CurrentThread.CurrentCulture?.Name;
            debugInfo.CurrentCulture.DisplayName = Thread.CurrentThread.CurrentCulture?.DisplayName;
            debugInfo.CurrentCulture.EnglishName = Thread.CurrentThread.CurrentCulture?.EnglishName;
            debugInfo.CurrentCulture.DateTimeFormat = Thread.CurrentThread.CurrentCulture?.DateTimeFormat?.ShortDatePattern + " " + Thread.CurrentThread.CurrentCulture?.DateTimeFormat?.ShortTimePattern;
            debugInfo.CurrentUICulture.Name = Thread.CurrentThread.CurrentUICulture?.Name;
            debugInfo.CurrentUICulture.DisplayName = Thread.CurrentThread.CurrentUICulture?.DisplayName;
            debugInfo.CurrentUICulture.EnglishName = Thread.CurrentThread.CurrentUICulture?.EnglishName;
            debugInfo.CurrentUICulture.DateTimeFormat = Thread.CurrentThread.CurrentUICulture?.DateTimeFormat?.ShortDatePattern + " " + Thread.CurrentThread.CurrentUICulture?.DateTimeFormat?.ShortTimePattern;
            debugInfo.Connection.Id = httpContext.Connection.Id;
            debugInfo.Connection.LocalIpAddress = httpContext.Connection.LocalIpAddress?.ToString();
            debugInfo.Connection.LocalPort = httpContext.Connection.LocalPort;
            debugInfo.Connection.RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            debugInfo.Connection.RemotePort = httpContext.Connection.RemotePort;
            debugInfo.Connection.ClientCertificate = httpContext.Connection.ClientCertificate?.ToString();
            debugInfo.Identities =
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

            return debugInfo;
        }
    }

    public class DebugInfo
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
