using CaseConverter;
using System.Reflection;

namespace RedBook.Core
{
    public static class ExtentionMethods
    {
        public static T ToObject<T>(this Dictionary<string, object> dictionary) where T : new()
        {
            T obj = new T();

            foreach (var kvp in dictionary)
            {
                PropertyInfo? property = typeof(T).GetProperty(kvp.Key.ToPascalCase());
                if (property != null && kvp.Value != null)
                {
                    object value = kvp.Value;
                    if (value != null)
                        property.SetValue(obj, value.ConvertTo(property.PropertyType), null);
                }
            }

            return obj;
        }

        private static object? ConvertTo(this object value, Type targetType)
        {
            try
            {
                string? valStr = value.ToString();
                if (string.IsNullOrEmpty(valStr)) return null;

                if (double.TryParse(valStr, out double doubleVal))
                {
                    switch (targetType.Name)
                    {
                        case "Int16":
                            return Convert.ToInt16(doubleVal);
                        case "Int32":
                            return Convert.ToInt32(doubleVal);
                        case "Int64":
                            return Convert.ToInt64(doubleVal);
                        case "Single":
                            return (float)doubleVal;
                        default:
                            return doubleVal;
                    }
                }

                if (bool.TryParse(valStr, out bool boolVal)) return boolVal;

                switch (targetType.Name)
                {
                    case "String":
                        return valStr;
                    case "Enum":
                        return Enum.Parse(targetType, valStr);
                    case "GUID":
                        return Guid.Parse(valStr);
                    case "DateTime":
                        return DateTime.Parse(valStr);
                    case "Single":
                        return bool.Parse(valStr);
                    default:
                        return Convert.ChangeType(value, targetType);
                }
            }
            catch
            {
                return null; // Return null if conversion fails
            }
        }

        private static object? ConvertTo<T>(this object value)
            => ConvertTo(value, typeof(T));
    }
}
