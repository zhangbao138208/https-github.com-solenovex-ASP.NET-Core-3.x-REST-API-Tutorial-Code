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
using Microsoft.Net.Http.Headers;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace RoutineApi.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public CompaniesController(
            ICompanyRepository companyRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService??throw new ArgumentNullException(nameof(propertyCheckerService));
        }
        [HttpGet(Name = nameof(GetCompanies))]
        public async Task<ActionResult> GetCompanies([FromQuery] CompanyDtoParameters parameters,
            [FromHeader(Name ="accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,out MediaTypeHeaderValue parseMediaType))
            {
                return BadRequest();  //返回状态码400
            }
            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(parameters.Fields))
            {
                return BadRequest();
            }
            if (!_propertyMappingService.ValidMappingExitsFor<CompanyDto,Company>(parameters.OrderBy))
            {
                return BadRequest();
            }
            var companies = await _companyRepository.GetCompaniesAsync(parameters);

            var paginationMetData = new
            {
                totalPages = companies.TotalPages,
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
             };

            Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(paginationMetData,
                new JsonSerializerOptions { 
                    Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));

            //是否是heteoas的dto
            bool whetherIncludeLinks = parseMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
            bool whetherIncludeFull = parseMediaType.SubTypeWithoutSuffix
                .ToString().Contains("full", StringComparison.InvariantCultureIgnoreCase);

            var links = CreateLinksForCompany(parameters, companies.HasPrevious, companies.HasNext);
            var shapedData = whetherIncludeFull ? _mapper.Map<IEnumerable<CompanyFullDto>>(companies)
                .ShapeData<CompanyFullDto>(parameters.Fields): _mapper.Map<IEnumerable<CompanyDto>>(companies)
                .ShapeData<CompanyDto>(parameters.Fields);

            if (whetherIncludeLinks)
            {
                var shapedCompaniesWithLinks = shapedData.Select(c =>
                {
                    var companyDict = c as IDictionary<string, object>;
                    var companyLinks = CreateLinksForCompany((Guid)companyDict["Id"], null);
                    companyDict.Add("links", companyLinks);
                    return companyDict;
                });

                var linkedCllectionResource = new
                {
                    value = shapedCompaniesWithLinks,
                    links
                };

                return Ok(linkedCllectionResource);
            }
            return Ok(shapedData);
        }

        [HttpGet("{companyId}", Name = nameof(GetCompany))]
        public async Task<IActionResult> GetCompany(Guid companyId,
            [FromQuery] string fields,
            [FromHeader(Name ="accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,out MediaTypeHeaderValue parseMediaType) )
            {
                return BadRequest();
            }
            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(fields))
            {
                return BadRequest();
            }
            var company = await _companyRepository.GetCompanyAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }

            //是否是heteoas的dto
            bool whetherIncludeLinks = parseMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas",StringComparison.InvariantCultureIgnoreCase);
            bool whetherIncludeFull = parseMediaType.SubTypeWithoutSuffix
                .ToString().Contains("full", StringComparison.InvariantCultureIgnoreCase);

            var shapedData = whetherIncludeFull ? _mapper.Map<CompanyFullDto>(company).ShapeData(fields) :
                _mapper.Map<CompanyDto>(company).ShapeData(fields);
            if (whetherIncludeLinks)
            {
                var links = CreateLinksForCompany(companyId, fields);
                IDictionary<string, object> linkDic = shapedData
                    as IDictionary<string, object>;
                linkDic.Add(nameof(links), links);
                return Ok(linkDic);
            }

            return Ok(shapedData);
        }

        [HttpPost(Name =nameof(CreateCompany))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyAddDto company)
        {
            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();
            var returnDto = _mapper.Map<CompanyDto>(entity);
            var shapedDto = returnDto.ShapeData<CompanyDto>(null) as IDictionary<string,object>;
            var links = CreateLinksForCompany(returnDto.Id,null);
            shapedDto.Add(nameof(links),links);
            return CreatedAtRoute(nameof(GetCompany), new { companyId = returnDto.Id }, shapedDto);
        }

        [HttpDelete("{companyId}",Name =nameof(DeleteCompany))]
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
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    });
                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm,
                        orderBy=parameters.OrderBy,
                        fields=parameters.Fields
                    });
                default:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        pageNumber = parameters.PageNumber ,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForCompany(Guid companyId,string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(
                    Url.Link(nameof(GetCompany),new { companyId}),
                    "self",
                    "Get"));
            }
            else
            {
                links.Add(new LinkDto(
                    Url.Link(nameof(GetCompany), new { companyId,fields }),
                    "self",
                    "Get"));
            }
            links.Add(new LinkDto(
                    Url.Link(nameof(DeleteCompany), new { companyId }),
                    "delete_company",
                    "Delete"));
            links.Add(new LinkDto(
                    Url.Link(nameof(EmployeesController.CreateEmployee), new { companyId }),
                    "create_employee",
                    "Post"));
            links.Add(new LinkDto(
                    Url.Link(nameof(EmployeesController.GetEmployees), new { companyId }),
                    "get_employee",
                    "Get"));
            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCompany(CompanyDtoParameters parameters,
            bool hasPrevious,
            bool hasNext)
        {
            List<LinkDto> links = new List<LinkDto>
            {
                new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.CurrentPage),
                "self",
                 "Get")
            };
            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage),
               "previous_page",
                "Get"));
            }
            if (hasNext)
            {
                links.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage),
               "next_page",
                "Get"));
            }

            return links;
        }
    }
}