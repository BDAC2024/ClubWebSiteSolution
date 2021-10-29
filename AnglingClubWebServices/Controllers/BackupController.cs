using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class BackupController : AnglingClubControllerBase
    {
        private readonly IBackupRepository _backupRepository;
        private readonly ILogger<BackupController> _logger;
        private readonly IMapper _mapper;

        public BackupController(
            IBackupRepository backupRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _backupRepository = backupRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<BackupController>();
            base.Logger = _logger;
        }

        [HttpGet("{itemsToBackup:int=-1}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BackupLine>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(int itemsToBackup)
        {
            StartTimer();

            var items = _backupRepository.Backup(itemsToBackup).Result;

            ReportTimer("Getting backup");

            return Ok(items);
        }


        /// <summary>
        /// Can be anonymous because it will not restore to a domian that has any data
        /// </summary>
        /// <param name="backupLines"></param>
        /// <param name="restoreToDomain"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{restoreToDomain}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Post([FromBody]List<BackupLine> backupLines, string restoreToDomain)
        {
            StartTimer();

            try
            {
                _backupRepository.Restore(backupLines, restoreToDomain).Wait();
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                ReportTimer("Restoring backup");
            }

        }


        [HttpDelete("{domainToClear}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Delete(string domainToClear)
        {
            StartTimer();

            try
            {
                _backupRepository.ClearDb(domainToClear).Wait();
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                ReportTimer("Clearing backup");
            }

        }
    }
}
