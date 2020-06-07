using AutoMapper;
using RoutineApi.Entites;
using RoutineApi.Models;

namespace RoutineApi.Profiles
{
    public class CompanyProfile:Profile
    {
        public CompanyProfile()
        {
            CreateMap<Company,CompanyDto>()
                .ForMember(des=>des.CompanyName,ops=>ops.MapFrom(src=>src.Name));
            CreateMap<CompanyAddDto, Company>();
            CreateMap<Company, CompanyFullDto>();
        }
    }
}
