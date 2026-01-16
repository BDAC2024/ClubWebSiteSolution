using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Models
{
    public sealed class DialogRequest
    {
        public DialogKind Kind { get; init; }
        public DialogSeverity Severity { get; init; }

        public string Title { get; init; } = "";
        public string Message { get; init; } = "";

        // Confirm-specific
        public string CancelText { get; init; } = "Cancel";
        public string ConfirmText { get; init; } = "OK";
        public Func<Task>? OnConfirmAsync { get; init; }
    }
}
