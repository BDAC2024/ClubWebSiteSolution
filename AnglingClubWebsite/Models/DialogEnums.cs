using Syncfusion.Blazor.Notifications;

namespace AnglingClubWebsite.Models
{
    public enum DialogKind
    {
        Confirm,
        Alert
    }

    public enum DialogSeverity
    {
        Info,
        Warn,
        Error,
        Success,
    }

    public static class DialogExtensions
    {
        public static MessageSeverity GetMessageSeverity(this DialogSeverity dialogSeverity)
        {
            MessageSeverity messageSeverity = MessageSeverity.Info;

            switch (dialogSeverity)
            {
                case DialogSeverity.Info:
                    messageSeverity = MessageSeverity.Info;
                    break;

                case DialogSeverity.Warn:
                    messageSeverity = MessageSeverity.Warning;
                    break;

                case DialogSeverity.Error:
                    messageSeverity = MessageSeverity.Error;
                    break;

                case DialogSeverity.Success:
                    messageSeverity = MessageSeverity.Success;
                    break;

                default:
                    break;
            }

            return messageSeverity;

        }
    }
}
