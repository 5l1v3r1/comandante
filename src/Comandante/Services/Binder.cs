using System;

namespace Comandante.Services
{
    public class Binder
    {
        public static string ConvertToString(object value)
        {
            if (value == null)
                return null;

            var sourceType = value.GetType();
            if (sourceType == typeof(DateTime) || sourceType == typeof(DateTime?))
            {
                return ((DateTime)value).ToString("yyyy-MM-ddTHH:mm");
            }
            if (sourceType == typeof(DateTimeOffset) || sourceType == typeof(DateTimeOffset?))
            {
                return ((DateTimeOffset)value).ToString("yyyy-MM-ddTHH:mm");
            }
            return value.ToString();
        }

        public static object ConvertToType(object obj, Type targetType)
        {
            string objStr = obj?.ToString();

            var targetUnderlyingType = targetType;
            if (targetType.IsNulable())
                targetUnderlyingType = Nullable.GetUnderlyingType(targetType);

            if (targetUnderlyingType.IsNumericType())
            {
                if (string.IsNullOrEmpty(objStr))
                {
                    if (targetType.IsNulable())
                        return Activator.CreateInstance(targetType, Activator.CreateInstance(targetUnderlyingType));
                    else
                        return Activator.CreateInstance(targetUnderlyingType);
                }

                long numericValue;
                if (long.TryParse(objStr, out numericValue))
                {
                    object targetValue = Convert.ChangeType(numericValue, targetUnderlyingType);
                    if (targetType.IsNulable())
                        return Activator.CreateInstance(targetType, targetValue);
                    else
                        return targetValue;
                }
                else
                    throw new ArgumentException($"Cannot convert value to {targetType.GetFriendlyName()}: {objStr}");
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (string.IsNullOrEmpty(objStr))
                {
                    if (targetType.IsNulable())
                        return null;
                    else
                        return Activator.CreateInstance(targetUnderlyingType);
                }

                bool boolValue;
                if (bool.TryParse(objStr, out boolValue))
                {
                    object targetUnderlyingValue = boolValue;
                    if (targetType.IsNulable())
                        return Activator.CreateInstance(targetType, targetUnderlyingValue);
                    else
                        return targetUnderlyingValue;
                }
                else
                    throw new ArgumentException($"Cannot convert value to {targetType.GetFriendlyName()}: {objStr}");
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (string.IsNullOrEmpty(objStr))
                {
                    if (targetType.IsNulable())
                        return null;
                    else
                        return Activator.CreateInstance(targetUnderlyingType);
                }

                DateTime dateTimeValue;
                if (DateTime.TryParse(objStr, out dateTimeValue))
                {
                    object targetUnderlyingValue = dateTimeValue;
                    if (targetType.IsNulable())
                        return Activator.CreateInstance(targetType, targetUnderlyingValue);
                    else
                        return targetUnderlyingValue;
                }
                else
                    throw new ArgumentException($"Cannot convert value to {targetType.GetFriendlyName()}: {objStr}");
            }

            if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
            {
                if (string.IsNullOrEmpty(objStr))
                {
                    if (targetType.IsNulable())
                        return null;
                    else
                        return Activator.CreateInstance(targetUnderlyingType);
                }

                DateTimeOffset dateTimeOffsetValue;
                if (DateTimeOffset.TryParse(objStr, out dateTimeOffsetValue))
                {
                    object targetUnderlyingValue = dateTimeOffsetValue;
                    if (targetType.IsNulable())
                        return Activator.CreateInstance(targetType, targetUnderlyingValue);
                    else
                        return targetUnderlyingValue;
                }
                else
                    throw new ArgumentException($"Cannot convert value to {targetType.GetFriendlyName()}: {objStr}");
            }

            if (targetType == typeof(string))
            {
                return objStr;
            }

            return obj;
        }

        public static bool IsConvertionSupported(Type targetType)
        {
            var targetUnderlyingType = targetType;
            if (targetType.IsNulable())
                targetUnderlyingType = Nullable.GetUnderlyingType(targetType);

            if (targetUnderlyingType.IsNumericType())
                return true;
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return true;
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return true;
            if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
                return true;
            if (targetType == typeof(string))
                return true;
            return false;
        }
    }
}
