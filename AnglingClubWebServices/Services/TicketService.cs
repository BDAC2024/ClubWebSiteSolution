using AnglingClubWebServices.Models;
using PdfSharpCore;
using System.Collections.Generic;
using System;
using AnglingClubWebServices.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using AnglingClubWebServices.Helpers;
using HtmlRenderer.NetCore.PdfSharp;
using System.Net.Mail;


namespace AnglingClubWebServices.Services
{
    public class TicketService : ITicketService
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketService> _logger;
        private readonly IDayTicketRepository _ticketRepository;
        public TicketService(IAppSettingRepository appSettingRepository,
            IEmailService emailService,
            ILoggerFactory loggerFactory,
            IDayTicketRepository ticketRepository)
        {
            _appSettingRepository = appSettingRepository;
            _emailService = emailService;
            _logger = loggerFactory.CreateLogger<TicketService>();
            _ticketRepository = ticketRepository;
        }

        public void IssueDayTicket(DateTime validOn, string holdersName, string emailAddress, string paymentId)
        {
            var appSettings = _appSettingRepository.GetAppSettings().Result;

            // Stripe may call the webhook more that once if the previous attempt failed. Ensure it is only issued once
            if (_ticketRepository.GetDayTickets().Result.Any(x => x.PaymentId == paymentId && x.IssuedOn != null))
            {
                _logger.LogWarning("Day ticket has already been issued. It cannot be issued again.");
                return;
            }

            var ticket = new DayTicket();
            if (ticket.IsNewItem)
            {
                var latestTicket = _ticketRepository.GetDayTickets().Result.OrderByDescending(x => x.TicketNumber).FirstOrDefault();
                ticket.TicketNumber = latestTicket != null ? latestTicket.TicketNumber + 1 : 1;
            }

            ticket.PaymentId = paymentId;

            _ticketRepository.AddOrUpdateTicket(ticket).Wait();

            var ticketStyle = appSettings.DayTicketStyle;

            var ticketHtml = String.Format(appSettings.DayTicket,
                appSettings.DayTicketCost.ToString("0.00"),
                ticket.TicketNumber,
                validOn.PrettyDate(),
                holdersName,
                ticketStyle
            );

            try
            {
                var pdfConfig = new PdfGenerateConfig();
                pdfConfig.PageOrientation = PageOrientation.Portrait;
                pdfConfig.PageSize = PageSize.A5;
                pdfConfig.MarginTop = 20;

                using (var pdf = PdfGenerator.GeneratePdf(ticketHtml, pdfConfig))
                {
                    using (var pdfStream = new System.IO.MemoryStream())
                    {
                        pdf.Save(pdfStream, false);

                        using (var imgStream = new System.IO.MemoryStream())
                        {
                            PDFtoImage.Conversion.SavePng(imgStream, pdfStream);

                            _emailService.SendEmail(
                                new List<string> { emailAddress },
                                "Your Day Ticket",
                                $"Please find attached, your day ticket valid for fishing on {validOn.PrettyDate()}.<br/>" +
                                    "Make sure you have your ticket with you when fishing. Either on your phone or printed.<br/><br/>" +
                                    "Tight lines!,<br/>" +
                                    "Boroughbridge & District Angling Club",
                                null,
                                new List<ImageAttachment>
                                {
                                    new ImageAttachment
                                    {
                                        Filename = $"Day_Ticket_{validOn:yyyy_MM_dd}.png",
                                        DataUrl = "data:image/png;base64," + Convert.ToBase64String(imgStream.ToArray())
                                    }
                                }
                            );

                            ticket.IssuedOn = DateTime.Now;
                            _ticketRepository.AddOrUpdateTicket(ticket).Wait();

                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Day ticket generation failed.");
            }
            finally
            {
                GC.Collect();
            }

        }

        public void IssueGuestTicket(DateTime validOn, string membersName, string guestsName, int membershipNumber, string emailAddress, string paymentId)
        {
            var appSettings = _appSettingRepository.GetAppSettings().Result;

            // Stripe may call the webhook more that once if the previous attempt failed. Ensure it is only issued once
            if (_ticketRepository.GetDayTickets().Result.Any(x => x.PaymentId == paymentId && x.IssuedOn != null))
            {
                _logger.LogWarning("Day ticket has already been issued. It cannot be issued again.");
                return;
            }

            var ticket = new DayTicket();
            if (ticket.IsNewItem)
            {
                var latestTicket = _ticketRepository.GetDayTickets().Result.OrderByDescending(x => x.TicketNumber).FirstOrDefault();
                ticket.TicketNumber = latestTicket != null ? latestTicket.TicketNumber + 1 : 1;
            }

            ticket.PaymentId = paymentId;

            _ticketRepository.AddOrUpdateTicket(ticket).Wait();

            var ticketStyle = appSettings.DayTicketStyle;

            var ticketHtml = String.Format(appSettings.DayTicket,
                appSettings.GuestTicketCost.ToString("0.00"),
                ticket.TicketNumber,
                validOn.PrettyDate(),
                guestsName + " THIS IS A GUEST",
                ticketStyle
            );

            try
            {
                var pdfConfig = new PdfGenerateConfig();
                pdfConfig.PageOrientation = PageOrientation.Portrait;
                pdfConfig.PageSize = PageSize.A5;
                pdfConfig.MarginTop = 20;

                using (var pdf = PdfGenerator.GeneratePdf(ticketHtml, pdfConfig))
                {
                    using (var pdfStream = new System.IO.MemoryStream())
                    {
                        pdf.Save(pdfStream, false);

                        using (var imgStream = new System.IO.MemoryStream())
                        {
                            PDFtoImage.Conversion.SavePng(imgStream, pdfStream);

                            _emailService.SendEmail(
                                new List<string> { emailAddress },
                                "Your Guest Ticket",
                                $"Please find attached, your guest ticket valid for fishing on {validOn.PrettyDate()}.<br/>" +
                                    "Make sure you have your ticket with you when fishing. Either on your phone or printed.<br/><br/>" +
                                    "Tight lines!,<br/>" +
                                    "Boroughbridge & District Angling Club",
                                null,
                                new List<ImageAttachment>
                                {
                                    new ImageAttachment
                                    {
                                        Filename = $"Guest_Ticket_{validOn:yyyy_MM_dd}.png",
                                        DataUrl = "data:image/png;base64," + Convert.ToBase64String(imgStream.ToArray())
                                    }
                                }
                            );

                            ticket.IssuedOn = DateTime.Now;
                            _ticketRepository.AddOrUpdateTicket(ticket).Wait();

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Day ticket generation failed.");
            }
            finally
            {
                GC.Collect();
            }

        }
    }
}
