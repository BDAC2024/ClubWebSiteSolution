using AnglingClubWebServices.Data;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class ProductMembershipsController : AnglingClubControllerBase
    {
        private readonly IProductMembershipRepository _productMembershipRepository;
        private readonly ILogger<ProductMembershipsController> _logger;
        private readonly IMapper _mapper;

        public ProductMembershipsController(
            IProductMembershipRepository productMembershipRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _productMembershipRepository = productMembershipRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<ProductMembershipsController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductMembership>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var data = _productMembershipRepository.GetProductMemberships().Result.OrderBy(x => x.Description).ThenBy(x => x.Term);

            ReportTimer("Getting ProductMemberships");

            return Ok(data);

        }

        // POST api/values
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody]List<ProductMembership> memberships)
        {
            StartTimer();
            var errors = new List<string>();

            try
            {
                foreach (var membership in memberships)
                {
                    try
                    {
                        await _productMembershipRepository.AddOrUpdateProductMembership(membership);
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"{membership.Type.EnumDescription()} - {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                errors.Add(ex.Message);

            }
            finally
            {
                ReportTimer("Posting ProductMemberships");

            }
            if (errors.Any())
            {
                return BadRequest(errors);
            }
            else
            {
                return Ok();
            }
        }


    }
}
