using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using Syncfusion.Blazor.Notifications;

namespace AnglingClubWebsite.Models
{
    public enum MessageState
    {
        Info = 0,
        Error,
        Warn,
        Success,
    }

    public record class TurnOnDebugMessages(bool YesOrNo);

    public record class LoggedIn(ClientMemberDto User);

    public record class BrowserChange();

    // No longer used, each ViewModel handles its own progress indication
    //public record class ShowProgress();
    //public record class HideProgress();

    public record class SelectMenuItem(string NavigateUrl);

    public record class ShowConsoleMessage(string Content);

    public record class ShowMessage(MessageState State, string Title, string Body, string? CloseButtonTitle = "Cancel", MessageButton? confirmationButtonDetails = null);

    public record MessageButton()
    {
        public string Label { get; set; } = "NOT SET";
        public Func<Task>? OnConfirmed { get; set; }

    }

    public static class MessageExtensions
    {
        public static DialogSeverity GetDialogSeverity(this MessageState msgSeverity)
        {
            DialogSeverity dlgSeverity = DialogSeverity.Info;

            switch (msgSeverity)
            {
                case MessageState.Info:
                    dlgSeverity = DialogSeverity.Info;
                    break;

                case MessageState.Warn:
                    dlgSeverity = DialogSeverity.Warn;
                    break;

                case MessageState.Error:
                    dlgSeverity = DialogSeverity.Error;
                    break;

                case MessageState.Success:
                    dlgSeverity = DialogSeverity.Success;
                    break;

                default:
                    break;
            }

            return dlgSeverity;

        }
    }

}
