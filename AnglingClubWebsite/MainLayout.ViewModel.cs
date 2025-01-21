using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite
{
    public partial class MainLayoutViewModel : ViewModelBase
    {
        public MainLayoutViewModel(
            IMessenger messenger) : base(messenger)
        {
            
        }

        [ObservableProperty]
        private string _testMessage = "Hello from MainLayoutViewModel";

    }
}
