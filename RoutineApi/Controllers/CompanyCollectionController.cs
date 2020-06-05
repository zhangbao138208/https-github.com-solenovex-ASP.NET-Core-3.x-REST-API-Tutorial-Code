using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RoutineApi.Entites;
using RoutineApi.Helps;
using RoutineApi.Models;
using RoutineApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Controllers
{
    [ApiController]
    [Route("api/companycollection")]
    public class CompanyCollectionController:ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompanyCollectionController(ICompanyRepository companyRepository,IMapper mapper)
        {
            _companyRepository = companyRepository??throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{ids}",Name =nameof(GetCompanyCollection))]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> 
            GetCompanyCollection([FromRoute][ModelBinder(BinderType =typeof(ArrayModelBinder))]
        IEnumerable<Guid> ids)
        {
            if (ids==null)
            {
                return BadRequest();
            }
            var entites = await _companyRepository.GetCompaniesAsync(ids.ToArray());
            if (ids.Count()!=entites.Count())
            {
                return NotFound();
            }

            var dtoToReturn = _mapper.Map<IEnumerable<CompanyDto>>(entites);
            return Ok(dtoToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<CompanyDto>>>
             CompanyCollection([FromBody] IEnumerable<CompanyAddDto> companies)
        {
            if (companies.Count()==0)
            {
                ModelState.AddModelError("数据源", "不能为空");
                return ValidationProblem();
            }
            
            var companyEntites = _mapper.Map<ICollection<Company>>(companies);

            foreach (var entity in companyEntites)
            {
                _companyRepository.AddCompany(entity);
            }
            var returnDto = _mapper.Map<IEnumerable<CompanyDto>>(companyEntites);

            var idsString = string.Join(",", companyEntites.Select(x => x.Id));
            await _companyRepository.SaveAsync();
            return CreatedAtRoute(nameof(GetCompanyCollection),new {ids= idsString }, returnDto);
        }

        
    }
}
