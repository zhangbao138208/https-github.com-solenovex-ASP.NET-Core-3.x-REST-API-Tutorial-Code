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
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public EmployeesController(ICompanyRepository companyRepository,IMapper mapper)
        {
            _companyRepository = companyRepository??throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper??throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet(Name =nameof(GetEmployees))]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(Guid companyId)
        {
            if (!await _companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }
            var empoyees = await _companyRepository.GetEmployeesAsync(companyId);
            return Ok(_mapper.Map<IEnumerable<EmployeeDto>>(empoyees));
        }
        [HttpGet("{employeeId}",Name =nameof(GetEmployee))]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid companyId,Guid employeeId)
        {
            var employee = await _companyRepository.GetEmployeeAsync(employeeId, companyId);
            return Ok(_mapper.Map<EmployeeDto>(employee));
        }

        [HttpPost(Name =nameof(CreateEmployee))]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(
            [FromRoute] Guid companyId,
            [FromBody] EmployeeAddDto employee)
        {
            if (!await _companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }
            var entity = _mapper.Map<Employee>(employee);
            _companyRepository.AddEmployee(companyId,entity);
            await _companyRepository.SaveAsync();
            var returnDto = _mapper.Map<EmployeeDto>(entity);
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
            if (!await _companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }

            var employeeEtity = await _companyRepository.GetEmployeeAsync(employeeId,companyId);
            if (employeeEtity==null)
            {
                var employee = _mapper.Map<Employee>(employeeUpdateDto);
                employee.Id = employeeId;
                _companyRepository.AddEmployee(companyId,employee);
                await _companyRepository.SaveAsync();

                var dtoToReturn = _mapper.Map<EmployeeDto>(employee);
                return CreatedAtRoute(nameof(GetEmployee),new { companyId=companyId,employeeId=employeeId},dtoToReturn);
            }

            _mapper.Map(employeeUpdateDto,employeeEtity);
            _companyRepository.UpdateEmployee(employeeEtity);
            await _companyRepository.SaveAsync();
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
            if (!await _companyRepository.CompanyExistAsync(companyId))
            {
                return NotFound();
            }

            var employeeEtity = await _companyRepository.GetEmployeeAsync(employeeId, companyId);
            if (employeeEtity == null)
            {
                var employeeDto = new EmployeeUpdateDto();
                patchDocument.ApplyTo(employeeDto, ModelState);
                if (!TryValidateModel(employeeDto))
                {
                    return ValidationProblem(ModelState);
                }

                var dtoToAdd = _mapper.Map<Employee>(employeeDto);
                dtoToAdd.Id = employeeId;
                _companyRepository.AddEmployee(companyId,dtoToAdd);
                await _companyRepository.SaveAsync();

                var dtoToReturn = _mapper.Map<EmployeeDto>(dtoToAdd);
                return CreatedAtRoute(nameof(GetEmployee),new { companyId,employeeId},dtoToReturn);
            }

            var dtoToPatch = _mapper.Map<EmployeeUpdateDto>(employeeEtity);
            patchDocument.ApplyTo(dtoToPatch,ModelState);
            if (!TryValidateModel(dtoToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(dtoToPatch,employeeEtity);
            _companyRepository.UpdateEmployee(employeeEtity);
            await _companyRepository.SaveAsync();
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