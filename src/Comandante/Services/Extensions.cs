using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comandante.Services
{
    public static class Extensions
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            if (obj == null)
                return;
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, propertyValue);
        }

        public static object InvokeMethod(this object obj, string methodName, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            MethodInfo method = FindMethod(obj.GetType(), methodName, methodParameters);
            return method.Invoke(obj, methodParameters);
        }

        public static object InvokeStaticMethod(this Type type, string methodName, params object[] methodParameters)
        {
            if (type == null)
                return null;
            MethodInfo method = FindMethod(type, methodName, methodParameters);
            return method.Invoke(null, methodParameters);
        }

        private static MethodInfo FindMethod(Type type, string methodName, object[] methodParameters)
        {
            MethodInfo method = null;
            methodParameters = methodParameters ?? new object[0];
            foreach (var typeMethod in type.GetMethods().Where(x => x.Name == methodName))
            {
                ParameterInfo[] typeMethodParameters = typeMethod.GetParameters();
                if (methodParameters.Length != typeMethodParameters.Length)
                    continue;

                var allSame = true;
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (typeMethodParameters[i].ParameterType != methodParameters[i].GetType() &&
                        typeMethodParameters[i].ParameterType.IsAssignableFrom(methodParameters[i].GetType()) == false)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    method = typeMethod;
                    break;
                }
            }
            return method;
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
    }
}
