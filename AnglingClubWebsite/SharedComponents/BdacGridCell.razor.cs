using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class BdacGridCell<TItem>
    {
        [Parameter, EditorRequired] public TItem Item { get; set; } = default!;
        [Parameter]
        public EventCallback<TItem> OnSelect
        {
            get; set;
        }

        // Pass in your existing computed class string, we’ll prefix it with bdac-rowcell
        [Parameter]
        public string? AdditionalClass
        {
            get; set;
        }

        // Optional: disable selection for some rows/cells
        [Parameter]
        public bool Disabled
        {
            get; set;
        }

        [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;

        private string CssClass
            => string.IsNullOrWhiteSpace(AdditionalClass)
                ? "bdac-rowcell"
                : $"bdac-rowcell {AdditionalClass}";

        private async Task HandleClick()
        {
            if (Disabled)
            {
                return;
            }

            if (OnSelect.HasDelegate)
            {
                await OnSelect.InvokeAsync(Item);
            }
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (Disabled)
            {
                return;
            }

            // Enter / Space activate like a button (nice for accessibility)
            if (e.Key is "Enter" or " ")
            {
                await HandleClick();
            }
        }

    }
}