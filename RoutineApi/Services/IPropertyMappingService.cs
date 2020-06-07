using System.Collections.Generic;

namespace RoutineApi.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool ValidMappingExitsFor<TSource, TDestination>(string orderBy);
    }
}
