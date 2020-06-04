using AutoMapper;
using RoutineApi.Entites;
using RoutineApi.Models;
using System;

namespace RoutineApi.Profiles
{
    public class EmployeeProfile:Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(des => des.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(des => des.GenderDisplay, opt => opt.MapFrom(src => Enum.GetName(typeof(Gender), src.Gender)))
                .ForMember(des=>des.Age,opt=>opt.MapFrom(src=>DateTime.Now.Year-src.DateBirthday.Year));
            CreateMap<EmployeeAddDto, Employee>();
            CreateMap<Employee,EmployeeUpdateDto>();
            CreateMap<EmployeeUpdateDto, Employee>();

        }
    }
}
