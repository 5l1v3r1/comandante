using System;
using System.Linq;
using System.Reflection;

namespace Comandante.Services
{
    public static class Reflector
    {
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
                return null;
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        public static object GetFieldValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;
            return
                (
                    obj.GetType().GetField(fieldName) ??
                    obj.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == fieldName)
                )?.GetValue(obj);
        }

        public static object GetPropertyOrFieldValue(object obj, string fieldName)
        {
            return GetPropertyValue(obj, fieldName) ?? GetFieldValue(obj, fieldName);
        }

        public static void SetPropertyValue(object obj, string propertyName, object propertyValue)
        {
            if (obj == null)
                return;
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, propertyValue);
        }

        public static object InvokeMethod(object obj, string methodName, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            MethodInfo method = FindMethod(obj.GetType(), methodName, methodParameters);
            return method.Invoke(obj, methodParameters);
        }

        public static object InvokeGenericMethod(object obj, string methodName, Type[] typeArguments, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            MethodInfo method = FindMethod(obj.GetType(), methodName, methodParameters);
            MethodInfo generic = method.MakeGenericMethod(typeArguments);
            return generic.Invoke(obj, methodParameters);
        }

        public static object InvokeStaticMethod(Type type, string methodName, params object[] methodParameters)
        {
            if (type == null)
                return null;
            MethodInfo method = FindMethod(type, methodName, methodParameters);
            return method.Invoke(null, methodParameters);
        }

        public static object InvokeStaticGenericMethod(Type type, string methodName, Type[] typeArguments, params object[] methodParameters)
        {
            if (type == null)
                return null;
            MethodInfo method = FindMethod(type, methodName, methodParameters);
            MethodInfo generic = method.MakeGenericMethod(typeArguments);
            return generic.Invoke(null, methodParameters);
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
            if (type.IsNulable())
                type = Nullable.GetUnderlyingType(type);
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
            return Nullable.GetUnderlyingType(type) != null;
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

        public static string GetFullFriendlyName(this Type type)
        {
            string friendlyName = type.FullName == null ? type.Name : (type.Namespace != null ? type.Namespace + "." + type.Name : type.Name);
            if (type.IsGenericType)
            {
                friendlyName += ("<" + string.Join(",", type.GetGenericArguments().Select(x => x.GetFullFriendlyName())) + ">");
            }

            return friendlyName;
        }
    }
}
