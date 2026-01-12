using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly ILogger<MeetingsController> _logger;
        private readonly ITmpFileRepository _tmpFileRepository;
        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMemberRepository _memberRepository;

        public MeetingsController(
            ILoggerFactory loggerFactory, ITmpFileRepository tmpFileRepository, IDocumentService documentService, IDocumentRepository documentRepository)
        {
            _logger = loggerFactory.CreateLogger<MeetingsController>();
            _tmpFileRepository = tmpFileRepository;
            _documentService = documentService;
            _documentRepository = documentRepository;
        }


    }
}
