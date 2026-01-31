using AnglingClubShared.DTOs;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;


namespace AnglingClubWebsite.Pages
{

    public partial class BookPrinting : RazorComponentBase
    {
        //private readonly IAuthenticationService? _authenticationService;
        //private readonly IMessenger? _messenger;
        //private readonly ICurrentUserService? _currentUserService;

        private readonly ITmpFileService _tmpFileService;
        private readonly IBookPrintingService _bookPrintingService;

        public BookPrinting(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ITmpFileService tmpFileService,
            IBookPrintingService bookPrintingService) : base(messenger, currentUserService, authenticationService)
        {
            _tmpFileService = tmpFileService;
            _bookPrintingService = bookPrintingService;
        }

        private UploadFiles? _file;

        private bool Uploading { get; set; } = false;
        private BookPrintingResult? Result { get; set; }

        public MarkupString ErrorMessage { get; set; }

        private bool CanRun => _file is not null;

        protected override void OnInitialized()
        {
            reset();
            base.OnInitialized();
        }
        private void RemoveHandler(RemovingEventArgs args)
        {
            reset();
        }
        private void UploadHandler(UploadChangeEventArgs args)
        {
            if (args.Files.Any())
            {
                ErrorMessage = new MarkupString(string.Empty);
                _file = args.Files.First();
                Result = null;
            }
        }

        private async Task Run()
        {
            ErrorMessage = new MarkupString(string.Empty);

            if (_file is null)
            {
                return;
            }

            Uploading = true;

            try
            {

                Result = await _bookPrintingService.GetPrintReadyPDFs(_file);

            }
            catch (ApiValidationException ex)
            {
                ErrorMessage = new MarkupString($"There was an error uploading the file: {ex.Message}");
            }
            catch (ApiNotFoundException ex)
            {
                ErrorMessage = new MarkupString($"There was an error uploading the file: {ex.Message}");
            }
            finally
            {
                Uploading = false;
            }


        }

        private void reset()
        {
            _file = null;
            Result = null;
            ErrorMessage = new MarkupString(string.Empty);
        }
    }
}
