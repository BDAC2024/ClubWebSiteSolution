using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class NewsController : AnglingClubControllerBase
    {
        private readonly INewsRepository _newsRepository;
        private readonly ILogger<NewsController> _logger;
        private readonly IMapper _mapper;

        public NewsController(
            INewsRepository newsRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _newsRepository = newsRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<NewsController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NewsItem>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var items = _newsRepository.GetNewsItems().Result;

            ReportTimer("Getting news items");

            return Ok(items);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]List<NewsItem> newsItems)
        {
            StartTimer();

            foreach (var newsItem in newsItems)
            {
                _newsRepository.AddOrUpdateNewsItem(newsItem);
            }

            ReportTimer("Posting news items");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var errors = new List<string>();

            try
            {
                _newsRepository.DeleteNewsItem(id).Wait();
            }
            catch (System.Exception)
            {
                throw;
            }

        }
    }
}
