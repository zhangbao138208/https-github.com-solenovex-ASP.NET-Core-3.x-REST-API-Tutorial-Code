using System.Reflection;

namespace RoutineApi.Services
{
    public class PropertyCheckerService : IPropertyCheckerService
    {
        public bool TypeHasProperties<TSource>(string fileds)
        {
            if (string.IsNullOrWhiteSpace(fileds))
            {
                return true;
            }
            var fieldAfterSplit = fileds.Split(',');
            foreach (var field in fieldAfterSplit)
            {
                var trimedField = field.Trim();
                var propertyInfo = typeof(TSource).GetProperty(trimedField, BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.IgnoreCase);
                if (propertyInfo == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
