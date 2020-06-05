using RoutineApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace RoutineApi.Helps
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source,
            string orderBy,
            Dictionary<string,PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }
            if (mappingDictionary==null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            //空字符串用来储存 OrderBy T-SQL 命令
            string orderQueryString = string.Empty;

            var orderByAfterSplit = orderBy.Split(',');
            foreach (var orderByClause in orderByAfterSplit)
            {
                var trimedOrderByClause = orderByClause.Trim();
                //是否降序Desc
                var orderDescending = trimedOrderByClause.EndsWith(" desc");
                var indexOfFirstSpace = trimedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? 
                    trimedOrderByClause : trimedOrderByClause.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new Exception($"没有找到key为{propertyName}的映射");
                }
                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue==null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }
                    //构造 order by T-SQL 命令
                    //与视频中的方法略有不同，这种方法不用 Revert() 两次，性能更好
                    if (orderQueryString.Length > 0)
                    {
                        orderQueryString += ",";
                    }
                    orderQueryString += destinationProperty + (orderDescending ? " descending" : " ascending");
                }
            }
            //执行 order by T-SQL 命令
            //需要安装 System.Linq.Dynamic.Core 包，才能使用以下代码
            source = source.OrderBy(orderQueryString);
            return source;
        }
    }
}
