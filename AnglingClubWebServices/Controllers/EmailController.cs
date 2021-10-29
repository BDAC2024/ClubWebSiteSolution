using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class EmailController : AnglingClubControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;

        public EmailController(
            IEmailService emailService,
            IAuthService authService,
            ILoggerFactory loggerFactory)
        {
            _emailService = emailService;
            _authService = authService;
            _logger = loggerFactory.CreateLogger<EmailController>();
            base.Logger = _logger;
        }

        [HttpPost]
        [Route("Test")]
        public IActionResult Test(string to, string subject, string body)
        {
            StartTimer();

            if (CurrentUser.Name == _authService.GetDeveloperName())
            {
                if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
                {
                    return BadRequest("Cannot send an email: To, Subject and Body required");
                }

                _emailService.SendEmail(new List<string> {to}, subject, body);
            }
            else
            {
                return Unauthorized();
            }

            return Ok();
        }

    }
}
