using RoutineApi.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoutineApi.DtoParameters;
using RoutineApi.Helps;

namespace RoutineApi.Services
{
    public interface ICompanyRepository
    {
        Task<PagedList<Company>> GetCompaniesAsync(CompanyDtoParameters parameters);
        Task<Company> GetCompanyAsync(Guid companyId);
        Task<IEnumerable<Company>> GetCompaniesAsync(Guid[] companyIds);
        void AddCompany(Company company);
        void UpdateCompany(Company company);
        void DeleteCompany(Company company);
        Task<bool> CompanyExistAsync(Guid companyId);

        Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId);
        Task<Employee> GetEmployeeAsync(Guid employeeId, Guid companyId);
        void AddEmployee(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
        void UpdateEmployee(Employee employee);

        Task<bool> SaveAsync();
    }
}
