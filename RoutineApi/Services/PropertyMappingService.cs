using RoutineApi.Entites;
using RoutineApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoutineApi.Services
{
    public class PropertyMappingService:IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _companyFullPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id",new PropertyMappingValue( new List<string> { "Id"} )},
                {"CompanyName",new PropertyMappingValue( new List<string> { "Name" } )},
                {"Introduction",new PropertyMappingValue( new List<string> { "Introduction" } )},
                {"Country",new PropertyMappingValue( new List<string> { "Country" } )},
                {"Product",new PropertyMappingValue( new List<string> { "Product" } )},
                {"Industry",new PropertyMappingValue( new List<string> { "Industry" } )}
            };

        private readonly Dictionary<string, PropertyMappingValue> _employeeFullPropertyMapping =
            new Dictionary<string, PropertyMappingValue>()
            {
                {"Id",new PropertyMappingValue(new List<string>{ "Id"}) },
                {"CompanyId",new PropertyMappingValue(new List<string>{ "CompanyId"}) },
                {"EmployeeNo",new PropertyMappingValue(new List<string>{ "EmployeeNo"}) },
                {"Name",new PropertyMappingValue(new List<string>{ "FirstName","LastName"}) },
                {"GenderDisplay",new PropertyMappingValue(new List<string>{ "Gender"}) },
                {"Age",new PropertyMappingValue(new List<string>{ "DateBirthday"},true) },
            };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();
        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<CompanyDto,Company>(_companyFullPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<EmployeeDto,Employee>(_employeeFullPropertyMapping));
        }

        public Dictionary<string,PropertyMappingValue> GetPropertyMapping<TSource,TDestination>()
        {
            var matchPropertyMapping = _propertyMappings.OfType<PropertyMapping<TSource,TDestination>>();
            matchPropertyMapping = matchPropertyMapping.ToList();
            if (matchPropertyMapping.Count()==1)
            {
                return matchPropertyMapping.First().MappingDictionary;
            }
            throw new Exception($"无法找到唯一的映射关系{typeof(TSource)},{typeof(TDestination)}");
        }
        
        public bool ValidMappingExitsFor<TSource, TDestination>(string orderBy)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return true;
            }
            var propertyMapping = GetPropertyMapping<TSource,TDestination>();

            var fieldAfterSplit = orderBy.Split(',');
            foreach (var field in fieldAfterSplit)
            {
                var trimedField = field.Trim();
                var indexOfFisrtSpace = trimedField.IndexOf(" ");
                var propertyName = indexOfFisrtSpace == -1 ? trimedField : trimedField.Remove(indexOfFisrtSpace);
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
