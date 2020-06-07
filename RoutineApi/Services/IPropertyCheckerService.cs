namespace RoutineApi.Services
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<TSource>(string fileds);
    }
}