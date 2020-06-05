using Microsoft.AspNetCore.Mvc;
using RoutineApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RoutineApi.Models;
using RoutineApi.DtoParameters;
using RoutineApi.Entites;
using RoutineApi.Helps;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace RoutineApi.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;

        public CompaniesController(ICompanyRepository companyRepository, IMapper mapper,
            IPropertyMappingService propertyMappingService)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }
        [HttpGet(Name = nameof(GetCompanies))]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies([FromQuery] CompanyDtoParameters parameters)
        {
            if (!_propertyMappingService.ValidMappingExitsFor<CompanyDto,Company>(parameters.OrderBy))
            {
                return BadRequest();
            }
            var companies = await _companyRepository.GetCompaniesAsync(parameters);

            var previousPageLink =companies.HasPrevious? CreateCompaniesResourceUri(parameters,ResourceUriType.PreviousPage):null;
            var nextPageLink = companies.HasNext? CreateCompaniesResourceUri(parameters,ResourceUriType.NextPage):null;
            var paginationMetData = new
            {
                totalPages = companies.TotalPages,
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(paginationMetData,
                new JsonSerializerOptions { 
                    Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            return Ok(_mapper.Map<IEnumerable<CompanyDto>>(companies));
        }

        [HttpGet("{companyId}", Name = nameof(GetCompany))]
        public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId)
        {
            var company = await _companyRepository.GetCompanyAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<CompanyDto>(company));
        }

        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CompanyAddDto company)
        {
            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();
            var returnDto = _mapper.Map<CompanyDto>(entity);
            return CreatedAtRoute(nameof(GetCompany), new { companyId = returnDto.Id }, returnDto);
        }

        [HttpDelete("{companyId}")]
        public async Task<ActionResult> DeleteCompany([FromRoute] Guid companyId)
        {
            var company = await _companyRepository.GetCompanyAsync(companyId);
            _companyRepository.DeleteCompany(company);
            await _companyRepository.SaveAsync();
            return NoContent();
        }
        
        private string CreateCompaniesResourceUri(CompanyDtoParameters parameters,ResourceUriType resourceUriType)
        {
            switch (resourceUriType)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link(nameof(GetCompanies),new {
                        pageNumber = parameters.PageNumber - 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm,
                    });
                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm,
                    });
                default:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber ,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm,
                    });
            }
        }
    }
}