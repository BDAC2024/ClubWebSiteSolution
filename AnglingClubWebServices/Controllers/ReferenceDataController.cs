using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class ReferenceDataController : ControllerBase
    {
        private readonly ILogger<ReferenceDataController> _logger;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IMapper _mapper;

        public ReferenceDataController(
            IReferenceDataRepository referenceDataRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _referenceDataRepository = referenceDataRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<ReferenceDataController>();
        }

        // GET api/values
        [HttpGet]
        public ReferenceData Get()
        {
            var refData = _referenceDataRepository.GetReferenceData();

            return refData;
        }
    }
}
