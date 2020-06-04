using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RoutineApi.Entites;
using RoutineApi.Models;
using RoutineApi.Services;

namespace RoutineApi.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IMapper mapper;

        public EmployeesController(ICompanyRepository companyRepository,IMapper mapper)
        {
            this.companyRepository = companyRepository??throw new ArgumentNullException(nameof(companyRepository));
            this.mapper = mapper??throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(Guid companyId)
        {
            if (!await companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }
            var empoyees = await companyRepository.GetEmployeesAsync(companyId);
            return Ok(mapper.Map<IEnumerable<EmployeeDto>>(empoyees));
        }
        [HttpGet("{employeeId}",Name =nameof(GetEmployee))]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid companyId,Guid employeeId)
        {
            var employee = await companyRepository.GetEmployeeAsync(employeeId, companyId);
            return Ok(mapper.Map<EmployeeDto>(employee));
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(
            [FromRoute] Guid companyId,
            [FromBody] EmployeeAddDto employee)
        {
            if (!await companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }
            var entity = mapper.Map<Employee>(employee);
            companyRepository.AddEmployee(companyId,entity);
            await companyRepository.SaveAsync();
            var returnDto = mapper.Map<EmployeeDto>(entity);
            return CreatedAtRoute(nameof(GetEmployee)
                ,new { employeeId = entity.Id,companyId=entity.CompanyId },returnDto);
        }

        [HttpPut("{employeeId}")]
        public async Task<ActionResult<CompanyDto>> UpdateEmployeeForCompany(
            [FromRoute] Guid companyId,
            [FromRoute] Guid employeeId,
            [FromBody] EmployeeUpdateDto employeeUpdateDto
            )
        {
            if (!await companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }

            var employeeEtity = await companyRepository.GetEmployeeAsync(employeeId,companyId);
            if (employeeEtity==null)
            {
                var employee = mapper.Map<Employee>(employeeUpdateDto);
                employee.Id = employeeId;
                companyRepository.AddEmployee(companyId,employee);
                await companyRepository.SaveAsync();

                var dtoToReturn = mapper.Map<EmployeeDto>(employee);
                return CreatedAtRoute(nameof(GetEmployee),new { companyId=companyId,employeeId=employeeId},dtoToReturn);
            }

            mapper.Map(employeeUpdateDto,employeeEtity);
            companyRepository.UpdateEmployee(employeeEtity);
            await companyRepository.SaveAsync();
            return NoContent();
        }

        /*
        * HTTP PATCH 举例（视频P32）
        * 原资源：
        *      {
        *        "baz":"qux",
        *        "foo":"bar"
        *      }
        * 
        * 请求的 Body:
        *      [
        *        {"op":"replace","path":"/baz","value":"boo"},
        *        {"op":"add","path":"/hello","value":["world"]},
        *        {"op":"remove","path":"/foo"}
        *      ]
        *      
        * 修改后的资源：
        *      {
        *        "baz":"boo",
        *        "hello":["world"]
        *      }
        *      
        * JSON PATCH Operations:
        * Add:
        *   {"op":"add","path":"/biscuits/1","value":{"name","Ginger Nut"}}
        * Replace:
        *   {"op":"replace","path":"/biscuits/0/name","value":"Chocolate Digestive"}
        * Remove:
        *   {"op":"remove","path":"/biscuits"}
        *   {"op":"remove","path":"/biscuits/0"}
        * Copy:
        *   {"op":"copy","from":"/biscuits/0","path":"/best_biscuit"}
        * Move:
        *   {"op":"move","from":"/biscuits","path":"/cookies"}
        * Test:
        *   {"op":"test","path":"/best_biscuit","value":"Choco Leibniz}
        */
        [HttpPatch("{employeeId}")]
        public async Task<ActionResult<EmployeeDto>> PartiallyUpdateEmloyeeForCompany(
            [FromRoute] Guid companyId,
            [FromRoute] Guid employeeId,
            JsonPatchDocument<EmployeeUpdateDto> patchDocument
            )
        {
            if (!await companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }

            var employeeEtity = await companyRepository.GetEmployeeAsync(employeeId, companyId);
            if (employeeEtity == null)
            {
                var employeeDto = new EmployeeUpdateDto();
                patchDocument.ApplyTo(employeeDto, ModelState);
                if (!TryValidateModel(employeeDto))
                {
                    return ValidationProblem(ModelState);
                }

                var dtoToAdd = mapper.Map<Employee>(employeeDto);
                dtoToAdd.Id = employeeId;
                companyRepository.AddEmployee(companyId,dtoToAdd);
                await companyRepository.SaveAsync();

                var dtoToReturn = mapper.Map<EmployeeDto>(dtoToAdd);
                return CreatedAtRoute(nameof(GetEmployee),new { companyId,employeeId},dtoToReturn);
            }

            var dtoToPatch = mapper.Map<EmployeeUpdateDto>(employeeEtity);
            patchDocument.ApplyTo(dtoToPatch,ModelState);
            if (!TryValidateModel(dtoToPatch))
            {
                return ValidationProblem(ModelState);
            }
            mapper.Map(dtoToPatch,employeeEtity);
            companyRepository.UpdateEmployee(employeeEtity);
            await companyRepository.SaveAsync();
            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(
                ControllerContext
                );
        }
    }
}