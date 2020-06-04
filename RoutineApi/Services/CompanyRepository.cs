using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoutineApi.Data;
using RoutineApi.DtoParameters;
using RoutineApi.Entites;

namespace RoutineApi.Services
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly RoutineDbContext _routineDbContext;

        public CompanyRepository(RoutineDbContext routineDbContext)
        {
            _routineDbContext = routineDbContext??throw new ArgumentNullException(nameof(routineDbContext));
        }
        public void AddCompany(Company company)
        {
            if (company==null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            company.Id = Guid.NewGuid();
            if (company.Employees!=null)
            {
                foreach (var employee in company.Employees)
                {
                    employee.Id = Guid.NewGuid();
                }
            }
            
            _routineDbContext.Companies.Add(company);
        }

        public void AddEmployee(Guid companyId, Employee employee)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }
            if (employee.Id==Guid.Empty)
            {
                employee.Id = Guid.NewGuid();
            }
            employee.CompanyId = companyId;
            _routineDbContext.Employees.Add(employee);
        }

        public async Task<bool> CompanyExistAsync(Guid companyId)
        {
            if (companyId ==Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _routineDbContext.Companies.AnyAsync(x=>x.Id==companyId);
        }

        public void DeleteCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            _routineDbContext.Remove(company);
        }

        public void DeleteEmployee(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            _routineDbContext.Remove(employee);
        }

        public async Task<IEnumerable<Company>> GetCompaniesAsync(CompanyDtoParameters parameters)
        {
            if (parameters==null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            var queryExpression= _routineDbContext.Companies as IQueryable<Company>;

            if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
            {
                parameters.CompanyName = parameters.CompanyName.Trim();
                queryExpression = queryExpression.Where(x=>x.Name==parameters.CompanyName);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                parameters.SearchTerm = parameters.SearchTerm.Trim();
                queryExpression = queryExpression
                    .Where(x => x.Name.Contains(parameters.SearchTerm)||x.Introduction.Contains(parameters.SearchTerm));

            }
            return await queryExpression.ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetCompaniesAsync(Guid[] companyIds)
        {
            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            return await _routineDbContext
                .Companies.Where(x=>companyIds.Contains(x.Id))
                .OrderBy(x=>x.Name)
                .ToListAsync();
        }

        public async Task<Company> GetCompanyAsync(Guid companyId)
        {
            if (companyId==Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _routineDbContext.Companies.FindAsync(companyId);
        }

        
       

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _routineDbContext.Employees
                .Where(x=>x.CompanyId==companyId)
                .OrderBy(x=>x.EmployeeNo)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeAsync(Guid employeeId, Guid companyId)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _routineDbContext.Employees
                .Where(x => x.CompanyId == companyId && x.Id == employeeId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAsync()
        {
            return await _routineDbContext.SaveChangesAsync() >= 0;
        }

        public void UpdateCompany(Company company)
        {
            _routineDbContext.Entry(company).State = EntityState.Modified;
        }

        public void UpdateEmployee(Employee employee)
        {
           _routineDbContext.Entry(employee).State = EntityState.Modified;
        }
    }
}
