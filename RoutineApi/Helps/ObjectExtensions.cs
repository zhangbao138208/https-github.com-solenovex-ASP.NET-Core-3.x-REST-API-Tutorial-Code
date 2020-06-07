using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace RoutineApi.Helps
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source,string fields)
        {
            if (source==null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            ExpandoObject expandoObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource).GetProperties(
                    BindingFlags.IgnoreCase|BindingFlags.Public|BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)expandoObject).Add(propertyInfo.Name,propertyValue);
                }
            }
            else
            {
                var fieldAfterSplit = fields.Split(',');
                foreach (var field in fieldAfterSplit)
                {
                    var trimedFeild = field.Trim();
                    var propertyInfo = typeof(TSource).GetProperty(trimedFeild,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo==null)
                    {
                        throw new Exception($"在{typeof(TSource)}上没有找到{trimedFeild}这个属性");
                    }
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)expandoObject).Add(propertyInfo.Name, propertyValue);
                }
            }
            return expandoObject;
        }
    }
}
