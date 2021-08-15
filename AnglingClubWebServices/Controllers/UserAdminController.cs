using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class UserAdminController : AnglingClubControllerBase
    {
        private readonly ILogger<UserAdminController> _logger;
        private readonly IUserAdminRepository _userAdminRepository;
        private readonly IMapper _mapper;

        public UserAdminController(
            IUserAdminRepository userAdminRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _userAdminRepository = userAdminRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<UserAdminController>();
            base.Logger = _logger;
        }

        // GET api/values
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserAdminContact>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var items = _userAdminRepository.GetUserAdmins().Result;

            ReportTimer("Getting user admins");

            return Ok(items);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]List<UserAdminContact> useAdmins)
        {
            StartTimer();

            foreach (var userAdmin in useAdmins)
            {
                _userAdminRepository.AddOrUpdateUserAdmin(userAdmin);
            }

            ReportTimer("Posting user admins");
        }


        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var errors = new List<string>();

            try
            {
                _userAdminRepository.DeleteUserAdmin(id).Wait();
            }
            catch (System.Exception)
            {
                throw;
            }

        }
    }
}
