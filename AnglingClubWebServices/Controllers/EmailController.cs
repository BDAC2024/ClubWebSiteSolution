using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe.Climate;
using Stripe;
using System.Collections.Generic;
using System.Web;
using System;

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
        public IActionResult Test(string to, string subject, string body, string canvasFilename = "", string canvasDataUrl = "")
        {
            StartTimer();

            if (CurrentUser.Name == _authService.GetDeveloperName())
            {
                if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
                {
                    return BadRequest("Cannot send an email: To, Subject and Body required");
                }

                List<ImageAttachment> canvasAttachments = null;

                if (!string.IsNullOrEmpty(canvasDataUrl) && ! string.IsNullOrEmpty(canvasFilename))
                {
                    canvasAttachments = new List<ImageAttachment>();
                    canvasAttachments.Add(new ImageAttachment
                    {
                        Filename = canvasFilename,
                        DataUrl = HttpUtility.UrlDecode(canvasDataUrl)
                    });
                }

                //var hardCodedDataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAACWCAYAAABkW7XSAAAAAXNSR0IArs4c6QAABzxJREFUeF7t3NuOGzkMRVHnzydf3gMbMRAkdluISElkrbxNd4WXfTgbhXrIj5s/CCCAQBECP4rMaUwEEEDgRliOAAEEyhAgrDJRGRQBBAjLDSCAQBkChFUmKoMigABhuQEEEChDgLDKRGVQBBAgLDeAAAJlCBBWmagMigAChOUGEECgDAHCKhOVQRFAgLDcAAIIlCFAWGWiMigCCBCWG0AAgTIECKtMVAZFAAHCcgMIIFCGAGGVicqgCCBAWG4AAQTKEIgW1n+32+3r1/Z/1n7+/P7rd7979fN3M97rvfrdu58/Q/mXv/fq77zb897nE4No7mUObnLQ+335c2EC0f/jOKgLH9OC1d3XAsgnt8gSlsM6OfV6sz3vyV3Vyy50YsIKxalYEgHCSgJbrSxhVUvsmvMS1jVz/2trwnIIFQgQVoWUFsxIWAsgazFNgLCmEfYoQFg9cuy+BWF1T3hwP8IaBOWxrQQIayv+c5oT1jlZmOQ9AcJyHQ8ChOUQKhAgrAopLZiRsBZA1mKaAGFNI+xRgLB65Nh9C8LqnvDgfoQ1CMpjWwkQ1lb85zQnrHOyMImP7m7gAwHCciIVCHjDqpDSghkJawFkLaYJENY0wh4FCKtHjt23IKzuCQ/uR1iDoDy2lQBhbcV/TnPCOicLk/jo7gZ8dHcDDQh4w2oQYsQK3rAiKKqRTYCwsgkXqU9YRYK6+JiEdfEDeK5PWA6hAgHCqpDSghkJawFkLaYJENY0wh4FCKtHjt23IKzuCQ/uR1iDoDy2lQBhbcV/TnPCOicLk7wnQFiu40GAsBxCBQKEVSGlBTMS1gLIWkwTIKxphD0KEFaPHLtvQVjdEx7cj7AGQXlsKwHC2or/nOaEdU4WJvHR3Q18IEBYTqQCAW9YFVJaMCNhLYCsxTQBwppG2KMAYfXIsfsWhNU94cH9CGsQlMe2EiCsrfjPaU5Y52RhEh/d3YCP7m6gAQFvWA1CjFjBG1YERTWyCRBWNuEi9QmrSFAXH5OwLn4Az/UJyyFUIEBYFVJaMCNhLYCsxTQBwppG2KMAYfXIsfsWhNU94cH9CGsQlMe2EiCsrfjPaU5Y52RhkvcECMt1PAgQlkOoQICwKqS0YEbCWgBZi2kChDWNsEcBwuqRY/ctCKt7woP7EdYgKI9tJUBYW/Gf05ywzsnCJD66u4EPBAjLiVQg4A2rQkoLZiSsBZC1mCZAWNMIexQgrB45dt+CsLonPLgfYQ2C8thWAoS1Ff85zQnrnCxM4qO7G/DR3Q00IOANq0GIESt4w4qgqEY2AcLKJlykPmEVCeriYxLWxQ/guT5hOYQKBAirQkoLZiSsBZC1mCZAWNMIexQgrB45dt/iLqyv2+32s/ui9vueAGG5kAoEvGFVSGnBjIS1ALIW0wQIaxphjwKE1SPH7lsQVveEB/cjrEFQHttKgLC24j+nOWGdk4VJ3hMgLNfxIEBYDqECAcKqkNKCGQlrAWQtpgkQ1jTCHgUIq0eO3bcgrO4JD+5HWIOgPLaVAGFtxX9Oc8I6JwuT+OjuBj4QICwnUoGAN6wKKS2YkbAWQNZimgBhTSPsUYCweuTYfQvC6p7w4H6ENQjKY1sJENZW/Oc0J6xzsjCJj+5uwEd3N9CAgDesBiFGrOANK4KiGtkECCubcJH6hFUkqIuPSVgXP4Dn+oTlECoQIKwKKS2YkbAWQNZimgBhTSPsUYCweuTYfQvC6p7w4H6ENQjKY1sJENZW/Oc0J6xzsjDJewKE5ToeBAjLIVQgQFgVUlowI2EtgKzFNAHCmkbYowBh9cix+xaE1T3hwf0IaxCUx7YSIKyt+M9pTljnZGESH93dwAcChOVEKhDwhlUhpQUzEtYCyFpMEyCsaYQ9ChBWjxy7b0FY3RMe3I+wBkF5bCsBwtqK/5zmhHVOFibx0d0N+OjuBhoQ8IbVIMSIFbxhRVBUI5sAYWUTLlKfsIoEdfExCeviB/Bcn7AcQgUChFUhpQUzEtYCyFpMEyCsaYQ9CmQJ63c6X7/9x6t+z9+/m+X++3/93b31n393pt53c7zqdf/Zp/17XNKaLZ7iWtNNl+MIZAnrOykcB8FAZQgQVpmocgaNFlbOlKoigAACCf/iKKgIIIBAGgFvWGloFUYAgWgChBVNVD0EEEgjQFhpaBVGAIFoAoQVTVQ9BBBII0BYaWgVRgCBaAKEFU1UPQQQSCNAWGloFUYAgWgChBVNVD0EEEgjQFhpaBVGAIFoAoQVTVQ9BBBII0BYaWgVRgCBaAKEFU1UPQQQSCNAWGloFUYAgWgChBVNVD0EEEgjQFhpaBVGAIFoAoQVTVQ9BBBII0BYaWgVRgCBaAKEFU1UPQQQSCNAWGloFUYAgWgChBVNVD0EEEgjQFhpaBVGAIFoAoQVTVQ9BBBII0BYaWgVRgCBaAKEFU1UPQQQSCNAWGloFUYAgWgChBVNVD0EEEgjQFhpaBVGAIFoAv8DBFrgl1J7+voAAAAASUVORK5CYII=";

                //System.Console.WriteLine(HttpUtility.UrlEncode(hardCodedDataUrl));

                _emailService.SendEmail(new List<string> {to}, subject, body, null, canvasAttachments);
            }
            else
            {
                return Unauthorized();
            }

            return Ok();
        }

        [HttpPost]
        [Route("TestBody")]
        public IActionResult TestBody([FromBody] EmailModel emailModel)
        {
            StartTimer();

            if (CurrentUser.Name == _authService.GetDeveloperName())
            {
                if (string.IsNullOrEmpty(emailModel.To) || string.IsNullOrEmpty(emailModel.Subject) || string.IsNullOrEmpty(emailModel.Body))
                {
                    return BadRequest("Cannot send an email: To, Subject and Body required");
                }

                _emailService.SendEmail(new List<string> { emailModel.To }, emailModel.Subject, emailModel.Body, null, emailModel.CanvasAttachments);
            }
            else
            {
                return Unauthorized();
            }

            return Ok();
        }

        [HttpGet]
        [Route("TestMembership")]
        public IActionResult TestmembershipEmail()
        {
            if (IsProd)
            {
                return BadRequest("Disabled in prod");
            }

            _emailService.SendEmail(
                new List<string> { "stve@townendmail.co.uk" },
                $"Confirmation of membership purchase",
                $"Thank you for purchasing <b>TEST MEMBERSHIP</b> .<br/>" +
                    "Your membership book will soon be prepared and will be sent to you when ready.<br/><br/>" +
                    "<b>Fishing is not permitted until membership book arrives.</b><br/><br/>" +
                    "Tight lines!,<br/>" +
                    "Boroughbridge & District Angling Club"
            );
            return Ok();
        }

        public class EmailModel
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public List<ImageAttachment> CanvasAttachments { get; set; }
        }
    }
}
