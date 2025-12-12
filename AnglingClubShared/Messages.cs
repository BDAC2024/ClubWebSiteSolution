using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubShared
{
    public record class TurnOnDebugMessages(bool YesOrNo);

    public record class LoggedIn(ClientMemberDto User);

    public record class BrowserChange();

    public record class ShowProgress();
    public record class HideProgress();
    public record class SelectMenuItem(string NavigateUrl);

    public record class ShowConsoleMessage(string Content);

    public record class ShowMessage(MessageState State, string Title, string Body, string? CloseButtonTitle = "Cancel", MessageButton? confirmationButtonDetails = null);

    public record MessageButton()
    {
        public string Label { get; set; } = "NOT SET";
        public Func<Task>? OnConfirmed { get; set; }

    }
}
