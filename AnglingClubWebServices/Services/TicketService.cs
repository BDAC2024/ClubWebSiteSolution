using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System;
using AnglingClubWebServices.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using AnglingClubWebServices.Helpers;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF.Drawing;


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

            QuestPDF.Settings.License = LicenseType.Community;

            string fontFilename = "arial.ttf";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream resFilestream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Fonts." + fontFilename))
            {
                if (resFilestream != null)
                {
                    FontManager.RegisterFont(resFilestream);
                }
                else
                {
                    throw new Exception($"CANNOT GET FILESTREAM OF FONT {fontFilename}");
                }
            }
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

            try
            {



                using (var pdfStream = new System.IO.MemoryStream())
                {
                    //var ticketPdf = generateDayTicket(holdersName, ticket.TicketNumber, validOn, appSettings.DayTicketCost);

                    ///*                    Document.Create(container =>
                    //                    {
                    //                        container.Page(page =>
                    //                        {
                    //                            page.Size(PageSizes.A4);
                    //                            page.MarginHorizontal(0.5f, Unit.Centimetre);
                    //                            page.MarginVertical(1f, Unit.Centimetre);

                    //                            page.DefaultTextStyle(y => y.FontFamily("Arial"));

                    //                            page.Content().Column(col =>
                    //                            {
                    //                                col.Item().HTML(handler =>
                    //                                {
                    //                                    handler.SetContainerStyleForHtmlElement("thead", c => c.Background(Colors.Blue.Lighten3));

                    //                                    handler.SetTextStyleForHtmlElement("th", TextStyle.Default.FontSize(20));
                    //                                    handler.SetContainerStyleForHtmlElement("th", c => c.AlignCenter());
                    //                                    handler.SetContainerStyleForHtmlElement("th", c => c.Background(Colors.Blue.Lighten3));

                    //                                    handler.SetTextStyleForHtmlElement("b", TextStyle.Default.Bold());

                    //                                    handler.SetTextStyleForHtmlElement("td", TextStyle.Default.FontSize(16)); 
                    //                                    handler.SetContainerStyleForHtmlElement("td", c => c.AlignCenter());

                    //                                    handler.SetHtml(ticketHtml);
                    //                                });
                    //                            });
                    //                        });
                    //                    }).GeneratePdf(pdfStream);
                    //                    */

                    //ticketPdf.GeneratePdf(pdfStream);

                    //using (var imgStream = new System.IO.MemoryStream())
                    //{
                    //    PDFtoImage.Conversion.SavePng(imgStream, pdfStream);

                    //    _emailService.SendEmail(
                    //        new List<string> { emailAddress },
                    //        "Your Day Ticket - NEW PDF",
                    //        $"Please find attached, your day ticket valid for fishing on {validOn.PrettyDate()}.<br/>" +
                    //            "Make sure you have your ticket with you when fishing. Either on your phone or printed.<br/><br/>" +
                    //            "Tight lines!,<br/>" +
                    //            "Boroughbridge & District Angling Club",
                    //        null,
                    //        new List<ImageAttachment>
                    //        {
                    //                new ImageAttachment
                    //                {
                    //                    Filename = $"Day_Ticket_{validOn:yyyy_MM_dd}.png",
                    //                    DataUrl = "data:image/png;base64," + Convert.ToBase64String(imgStream.ToArray())
                    //                }
                    //        }
                    //    );

                    //    ticket.IssuedOn = DateTime.Now;
                    //    _ticketRepository.AddOrUpdateTicket(ticket).Wait();

                    //}

                    ImageGenerationSettings settings = new ImageGenerationSettings();
                    settings.ImageFormat = ImageFormat.Png;
                    settings.RasterDpi = 800;
                    settings.ImageCompressionQuality = ImageCompressionQuality.Best;

                    var ticketImgPdf = generateDayTicket(holdersName, ticket.TicketNumber, validOn, appSettings.DayTicketCost);
                    var ticketImages = ticketImgPdf.GenerateImages(settings);

                    _emailService.SendEmail(
                        new List<string> { emailAddress },
                        "Your Day Ticket - NEW PDF as IMG 600DPI",
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
                                DataUrl = "data:image/png;base64," + Convert.ToBase64String(ticketImages.First())
                            }
                        }
                    );

                }

                /*
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
                */

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

        public void IssueGuestTicket(DateTime validOn, DateTime issuedOn, string membersName, string guestsName, int membershipNumber, string emailAddress, string paymentId)
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

            //var ticketStyle = appSettings.DayTicketStyle;

            //var ticketHtml = String.Format(appSettings.DayTicket,
            //    appSettings.GuestTicketCost.ToString("0.00"),
            //    ticket.TicketNumber,
            //    validOn.PrettyDate(),
            //    guestsName + " THIS IS A GUEST",
            //    ticketStyle
            //);

            try
            {
                ImageGenerationSettings settings = new ImageGenerationSettings();
                settings.ImageFormat = ImageFormat.Png;
                settings.RasterDpi = 800;
                settings.ImageCompressionQuality = ImageCompressionQuality.Best;

                var ticketImgPdf = generateGuestTicket(membersName, guestsName, ticket.TicketNumber, validOn, appSettings.DayTicketCost);
                var ticketImages = ticketImgPdf.GenerateImages(settings);

                _emailService.SendEmail(
                    new List<string> { emailAddress },
                    "Your Day Ticket - NEW PDF as IMG 600DPI",
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
                                DataUrl = "data:image/png;base64," + Convert.ToBase64String(ticketImages.First())
                            }
                    }
                );

                var ticketImgPdfOrig = generateGuestTicketOriginal(membersName, guestsName, ticket.TicketNumber, issuedOn, validOn, appSettings.DayTicketCost);
                var ticketImagesOrig = ticketImgPdfOrig.GenerateImages(settings);

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
                                DataUrl = "data:image/png;base64," + Convert.ToBase64String(ticketImagesOrig.First())
                            }
                    }
                );

                //var pdfConfig = new PdfGenerateConfig();
                //pdfConfig.PageOrientation = PageOrientation.Portrait;
                //pdfConfig.PageSize = PdfSharpCore.PageSize.A5;
                //pdfConfig.MarginTop = 20;

                //using (var pdf = PdfGenerator.GeneratePdf(ticketHtml, pdfConfig))
                //{
                //    using (var pdfStream = new System.IO.MemoryStream())
                //    {
                //        pdf.Save(pdfStream, false);

                //        using (var imgStream = new System.IO.MemoryStream())
                //        {
                //            PDFtoImage.Conversion.SavePng(imgStream, pdfStream);

                //            _emailService.SendEmail(
                //                new List<string> { emailAddress },
                //                "Your Guest Ticket",
                //                $"Please find attached, your guest ticket valid for fishing on {validOn.PrettyDate()}.<br/>" +
                //                    "Make sure you have your ticket with you when fishing. Either on your phone or printed.<br/><br/>" +
                //                    "Tight lines!,<br/>" +
                //                    "Boroughbridge & District Angling Club",
                //                null,
                //                new List<ImageAttachment>
                //                {
                //                    new ImageAttachment
                //                    {
                //                        Filename = $"Guest_Ticket_{validOn:yyyy_MM_dd}.png",
                //                        DataUrl = "data:image/png;base64," + Convert.ToBase64String(imgStream.ToArray())
                //                    }
                //                }
                //            );

                //            ticket.IssuedOn = DateTime.Now;
                //            _ticketRepository.AddOrUpdateTicket(ticket).Wait();

                //        }

                //    }
                //}

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

        private Document generateGuestTicketOriginal(string membersName, string ticketHolder, int ticketNumber, DateTime issuedOn, DateTime validOn, decimal price)
        {
            var margin = 1f;
            var headerHeight = 1.28f;
            var dateHeight = 1.07f;
            var notesHeight = 3.84f;
            var verticalPadding = 2f;

            var document = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(12.4f + (margin * 2), headerHeight + dateHeight + notesHeight + (margin * 2), QuestPDF.Infrastructure.Unit.Centimetre);
                    page.Margin(margin, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.DefaultTextStyle(y => y.FontFamily("Arial").FontSize(10));

                    page.Content()
                    .Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(4f, Unit.Centimetre);
                                columns.RelativeColumn(1);
                            });

                            table.Cell().Border(0.5f).AlignMiddle().Column(column =>
                            {
                                column.Item().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                    });

                                    table.Cell().Height(30).Border(0.5f).AlignMiddle().AlignCenter().Text($"£{price.ToString("0.00")}");

                                    table.Cell().Column(column =>
                                    {
                                        column.Item().PaddingTop(verticalPadding * 2.5f).AlignCenter().Text(text =>
                                        {
                                            text.Line("TICKET NO.");
                                            text.Span($"Online/{ticketNumber}").Bold();
                                            text.Line("");
                                            table.Cell().AlignMiddle().AlignTop().PaddingBottom(verticalPadding * 4).PaddingTop(verticalPadding * 3).Column(column =>
                                            {
                                                column.Item().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(7)).Table(nameTable =>
                                                {
                                                    nameTable.ColumnsDefinition(columns =>
                                                    {
                                                        columns.RelativeColumn(1);
                                                        columns.RelativeColumn(1.55f);
                                                    });

                                                    nameTable.Cell().AlignRight().Text("Issued by: ");
                                                    nameTable.Cell().AlignLeft().Text("Website");

                                                    nameTable.Cell().AlignRight().Text("Issued on: ");
                                                    nameTable.Cell().AlignLeft().Text(issuedOn.PrettyDate());
                                                });
                                            });
                                        });
                                    });

                                    table.Cell().Height(65).DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Border(0.5f).Column(column =>
                                    {
                                        column.Item().PaddingTop(verticalPadding * 2.5f).AlignCenter().Text(text =>
                                        {
                                            text.Line("Ticket Covers:");
                                            text.Line("");
                                            text.Line(validOn.PrettyDate()).Bold();
                                        });
                                    });
                                });
                            });

                            table.Cell().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Border(0.5f).AlignCenter().Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding * 4.5f).AlignCenter().Text(text =>
                                {
                                    text.Line("BOROUGHBRIDGE & DISTRICT ANGLING CLUB").Bold();
                                    text.Line("");
                                    text.Line("MEMBERS GUEST TICKET").FontSize(10).Bold();
                                    text.Line("").FontSize(2);
                                    text.Line("for the");
                                    text.Line("CLUB WATERS: RIVER URE & ROECLIFFE POND");
                                });

                                column.Item().PaddingLeft(8).DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Table(nameTable =>
                                {
                                    nameTable.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(2.5f, Unit.Centimetre);
                                        columns.RelativeColumn(1);
                                    });

                                    nameTable.Cell().AlignLeft().Text("MEMBERS NAME: ");
                                    nameTable.Cell().AlignLeft().Text(membersName).Bold();
                                    nameTable.Cell().PaddingTop(6).AlignLeft().Text("GUESTS NAME: ");
                                    nameTable.Cell().PaddingTop(6).AlignLeft().Text(ticketHolder).Bold();
                                });

                                column.Item().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(6.5f)).PaddingLeft(8).PaddingTop(verticalPadding * 7).AlignLeft().Text(text =>
                                {
                                    text.Line("NO TICKETS AVAILABLE ON ANY SUNDAY MATCH VENUES").FontSize(8);
                                    text.Span("Please read the pond rules and bait bans on the notice board ");
                                    text.Span("before").FontColor(Colors.Red.Darken4).Bold().Underline();
                                    text.Span(" fishing.");
                                    text.Line("");
                                    text.Line("Members must fish with their guest and be responsible for them.");
                                    text.Line("NO FISHING from boats either moving or static/moored.");

                                });
                            });
                        });
                    });
                });

            });

            return document;
        }
        private Document generateGuestTicket(string membersName, string ticketHolder, int ticketNumber, DateTime validOn, decimal price)
        {
            var margin = 1f;
            var headerHeight = 1.28f;
            var dateHeight = 1.07f;
            var notesHeight = 3.84f;
            var verticalPadding = 2f;

            var document = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(10.4f + (margin * 2), headerHeight + dateHeight + notesHeight + (margin * 2), QuestPDF.Infrastructure.Unit.Centimetre);
                    page.Margin(margin, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.DefaultTextStyle(y => y.FontFamily("Arial").FontSize(10));

                    page.Content()
                    .Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                            });

                            table.Cell().Height(headerHeight, QuestPDF.Infrastructure.Unit.Centimetre).Border(0.5f).AlignMiddle().Column(column =>
                            {
                                column.Item().Table(table =>
                                {
                                    generateTableHeader(table, headerHeight, "Guest", price);
                                });
                            });

                            table.Cell().AlignMiddle().Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding).AlignCenter().Text("for the CLUB WATERS: RIVER URE & ROECLIFFE POND");
                            });

                            table.Cell().AlignMiddle().Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding).PaddingBottom(verticalPadding).AlignCenter().Text(text =>
                                {
                                    text.Span("NO: ");
                                    text.Span($"online/{ticketNumber}   ").Bold();
                                    text.Span("DATED: ");
                                    text.Span(validOn.PrettyDate()).Bold();
                                });
                            });

                            table.Cell().AlignMiddle().Border(0.5f).Column(column =>
                            {
                                column.Item().Table(nameTable =>
                                {
                                    nameTable.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    nameTable.Cell().AlignRight().Text("MEMBERS NAME: ");
                                    nameTable.Cell().AlignLeft().Text(membersName).Bold();

                                    nameTable.Cell().AlignRight().Text("GUESTS NAME: ");
                                    nameTable.Cell().AlignLeft().Text(ticketHolder).Bold();
                                });
                            });

                            table.Cell().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding * 2.5f).AlignCenter().Text(text =>
                                {
                                    text.Line("NO TICKETS AVAILABLE ON ANY SUNDAY MATCH VENUES");
                                    text.Span("Please read the pond rules and bait bans on the notice board ");
                                    text.Span("before").FontColor(Colors.Red.Darken4).Bold().Underline();
                                    text.Span(" fishing.");
                                    text.Line("");
                                    text.Line("Members must fish with their guest and be responsible for them.");
                                    text.Line("NO FISHING from boats either moving or static/moored.");

                                });
                            });

                        });
                    });
                });

            });

            return document;
        }
        private Document generateDayTicket(string ticketHolder, int ticketNumber, DateTime validOn, decimal price)
        {
            var margin = 1f;
            var headerHeight = 1.28f;
            var dateHeight = 1.07f;
            var notesHeight = 3.84f;
            var verticalPadding = 2f;

            var document = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(10.4f + (margin * 2), headerHeight + dateHeight + notesHeight + (margin * 2), QuestPDF.Infrastructure.Unit.Centimetre);
                    page.Margin(margin, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.DefaultTextStyle(y => y.FontFamily("Arial").FontSize(10));

                    page.Content()
                    .Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                            });

                            table.Cell().Height(headerHeight, QuestPDF.Infrastructure.Unit.Centimetre).Border(0.5f).AlignMiddle().Column(column =>
                            {
                                column.Item().Table(table =>
                                {
                                    generateTableHeader(table, headerHeight, "Day", price);
                                });
                            });

                            table.Cell().AlignMiddle().Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding).PaddingBottom(verticalPadding).AlignCenter().Text(text =>
                                {
                                    text.Span("NO: ");
                                    text.Span($"online/{ticketNumber}   ").Bold();
                                    text.Span("DATED: ");
                                    text.Span(validOn.PrettyDate()).Bold();
                                });
                            });
                            table.Cell().AlignMiddle().Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding).PaddingBottom(verticalPadding).AlignCenter().Text(text =>
                                {
                                    text.Span("TICKET HOLDER: ");
                                    text.Span(ticketHolder).Bold();
                                });
                            });

                            table.Cell().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(8)).Border(0.5f).Column(column =>
                            {
                                column.Item().PaddingTop(verticalPadding * 2.5f).AlignCenter().Text(text =>
                                {
                                    text.Line("No dogs, No fires; No camping; No loud music.");
                                    text.Line("NO FISHING AFTER DUSK OR FROM BOATS.");
                                    text.Line("Entry to fishing via old Cricket Field & Hall Arms Lane.");
                                    text.Line("Fish old Cricket Field, peg 1 to limit board at");
                                    text.Line("Aldborough, which is one field upstream from Hall Arms");
                                    text.Line("Lane. (look for the limit board).");
                                    text.Line("Ticket Holder ONLY permitted to fish and ONLY on the date above.");
                                    text.Line("NO Live baiting, NO trolling & NO fish to be taken!");
                                });
                            });

                        });
                    });
                });

            });

            return document;
        }

        private void generateTableHeader(TableDescriptor headerTable, float headerHeight, string ticketType, decimal price)
        {
            string logoFilename = "logo.png";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream resFilestream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Fonts." + logoFilename))
            {
                if (resFilestream != null)
                {
                    headerTable.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(headerHeight * 1.2f, QuestPDF.Infrastructure.Unit.Centimetre);
                        columns.RelativeColumn(1);
                    });

                    headerTable.Cell().Background("#9DC3E6").AlignCenter().Image(resFilestream).FitHeight();

                    headerTable.Cell().Background("#9DC3E6").AlignMiddle().DefaultTextStyle(y => y.FontFamily("Arial").FontSize(10)).Column(headerColumn =>
                    {
                        headerColumn.Item().AlignCenter().Text("BOROUGHBRIDGE & DISTRICT ANGLING CLUB").Bold();
                        headerColumn.Item().AlignCenter().Text($"Coarse Fishing {ticketType} Ticket £{price.ToString("0.00")}").Bold();
                    });
                }
                else
                {
                    throw new Exception($"CANNOT GET FILESTREAM OF LOGO in fonts folder: {logoFilename}");
                }

            }

        }
    }
}
