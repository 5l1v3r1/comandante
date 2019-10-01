using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comandante.Services
{
    public static class Extensions
    {
        public static string GetAllMessages(this Exception ex)
        {
            if (ex is TargetInvocationException)
                ex = ex.InnerException;

            List<string> messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            messages.Reverse();
            return string.Join(". ", messages);
        }

        public static string GetAllDetails(this Exception ex)
        {
            if (ex is TargetInvocationException)
                ex = ex.InnerException;

            List<string> details = new List<string>();
            while (ex != null)
            {
                details.Add(ex.Message);
                details.Add(ex.StackTrace);
                ex = ex.InnerException;
            }
            return string.Join("\n\n", details);
        }

        public static string ToHumanString(this TimeSpan timeSpan)
        {
            StringBuilder sb = new StringBuilder();
            if (timeSpan.Days >= 1)
                sb.AppendFormat("{0}d, ", timeSpan.Days);
            if (timeSpan.Hours >= 1)
                sb.AppendFormat("{0}h, ", timeSpan.Hours);
            if (timeSpan.Minutes >= 1)
                sb.AppendFormat("{0}m, ", timeSpan.Minutes);
            if (timeSpan.Seconds >= 1)
                sb.AppendFormat("{0}s ", timeSpan.Seconds);
            if (timeSpan.Milliseconds >= 1)
                sb.AppendFormat("{0}ms ", timeSpan.Milliseconds);
            return sb.ToString();
        }

        public static int? ToIntOrDefault(this string text)
        {
            int number;
            if (int.TryParse(text, out number))
                return number;
            return null;

        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                 
                //ContractResolver = new IgnoreSystemNamespaceResolver(),
                //PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
            });
        }
    }

    //class IgnoreSystemNamespaceResolver : DefaultContractResolver
    //{
    //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //    {
    //        JsonProperty prop = base.CreateProperty(member, memberSerialization);

    //        if (prop.PropertyType.IsGenericType)
    //        {
    //            return prop;
    //        }

    //        if (prop.PropertyType.Namespace.StartsWith("System") == false)
    //        {
    //            return prop;
    //        }

    //        prop.Ignored = true;
    //        return prop;
    //    }
    //}
}
