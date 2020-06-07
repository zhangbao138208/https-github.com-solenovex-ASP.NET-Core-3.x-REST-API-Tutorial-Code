using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace RoutineApi.Helps
{
    public static class IEumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source,string fields)
        {
            if (source==null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var expandoObjectList = new List<ExpandoObject>(source.Count());
            var propertyInfoList = new List<PropertyInfo>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public|BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var filedAfterSplit = fields.Split(',');
                foreach (var field in filedAfterSplit)
                {
                    var trimedField = field.Trim();
                    var propertyName = typeof(TSource).GetProperty(trimedField,
                        BindingFlags.Instance|BindingFlags.IgnoreCase|BindingFlags.Public);
                    if (propertyName==null)
                    {
                        throw new Exception($"Property:{propertyName} 没有找到：{typeof(TSource)}");
                    }
                    propertyInfoList.Add(propertyName);
                }

            }

            foreach (TSource obj in source)
            {
                var expandoObject = new ExpandoObject();
                foreach (var propertyInfo in propertyInfoList)
                {
                    var propertyValue= propertyInfo.GetValue(obj);
                    ((IDictionary<string, object>)expandoObject).Add(propertyInfo.Name,propertyValue);
                }
                expandoObjectList.Add(expandoObject);
            }
            return expandoObjectList;
        }
    }
}
