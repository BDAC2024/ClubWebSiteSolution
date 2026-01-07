using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MeetingsController>();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Minutes()
        {
            var fileName = "Meetings/AGM_Minutes_2024.pdf";
            byte[] bytes = new byte[0];
            return File(bytes, "application/pdf", Path.ChangeExtension(fileName, ".pdf"));
        }
    }
}
