using System;
using System.Collections.Generic;
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

        public static object GetFieldValue(this object obj, string fieldName)
        {
            if (obj == null)
                return null;
            return 
                (
                    obj.GetType().GetField(fieldName) ??
                    obj.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == fieldName)
                )?.GetValue(obj);
        }

        public static object GetPropertyOrFieldValue(this object obj, string fieldName)
        {
            return GetPropertyValue(obj, fieldName) ?? GetFieldValue(obj, fieldName);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            if (obj == null)
                return;
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, propertyValue);
        }

        public static void SetPropertyValueAndConvertIfNeeded(this object obj, string propertyName, object propertyValue)
        {
            if (obj == null)
                return;
            var property = obj.GetType().GetProperty(propertyName);
            property.SetValue(obj, propertyValue.ConvertIfNeeded(property.PropertyType));
        }

        public static object ConvertIfNeeded(this object obj, Type type)
        {
            if (obj == null)
                return null;
            if (type.IsNumericType())
            {
                long numericValue;
                if (long.TryParse(obj?.ToString(), out numericValue))
                    return Convert.ChangeType(numericValue, type);
                else if (type.IsNulable())
                    return Activator.CreateInstance(type);
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                bool boolValue;
                if (bool.TryParse(obj?.ToString(), out boolValue))
                    return boolValue;
                else if (type.IsNulable())
                    return null;
                else
                    return Activator.CreateInstance(type);
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                DateTime dateTimeValue;
                if (DateTime.TryParse(obj?.ToString(), out dateTimeValue))
                    return dateTimeValue;
                else if (type.IsNulable())
                    return null;
                else
                    return Activator.CreateInstance(type);
            }
            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                DateTimeOffset dateTimeOffsetValue;
                if (DateTimeOffset.TryParse(obj?.ToString(), out dateTimeOffsetValue))
                    return dateTimeOffsetValue;
                else if (type.IsNulable())
                    return null;
                else
                    return Activator.CreateInstance(type);
            }
            return obj;
        }

        public static string GetDetails(this Exception ex)
        {
            List<string> messages = new List<string>();
            while(ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            messages.Reverse();
            return string.Join(". ", messages);
        }

        public static object InvokeMethod(this object obj, string methodName, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            MethodInfo method = FindMethod(obj.GetType(), methodName, methodParameters);
            return method.Invoke(obj, methodParameters);
        }

        public static object InvokeGenericMethod(this object obj, string methodName, Type[] typeArguments, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            MethodInfo method = FindMethod(obj.GetType(), methodName, methodParameters);
            MethodInfo generic = method.MakeGenericMethod(typeArguments);
            return generic.Invoke(obj, methodParameters);
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

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNulable(this Type type)
        {
            return type == typeof(Nullable<>);
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                friendlyName += ("<" + string.Join(",", type.GetGenericArguments().Select(x => x.GetFriendlyName())) + ">");
            }

            return friendlyName;
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
