using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace AnglingClubWebsite.SharedComponents
{
    public abstract class MvvmComponentBase<TViewModel>: ComponentBase
        where TViewModel : IViewModelBase
    {
        [Inject]
        [NotNull]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected TViewModel ViewModel { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected override void OnInitialized()
        {
            // Cause changes to the viewmodel to make blazor re-render
            ViewModel.PropertyChanged += (_, _) => StateHasChanged();
            base.OnInitialized();
        }

        protected override Task OnInitializedAsync()
        {
            return ViewModel.OnInitializedAsync();
        }
    }
}
