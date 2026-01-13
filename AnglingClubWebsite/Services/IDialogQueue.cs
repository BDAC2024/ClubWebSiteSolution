using AnglingClubWebsite.Models;

namespace AnglingClubWebsite.Services
{
    public interface IDialogQueue
    {
        void Enqueue(DialogRequest request);
        bool TryDequeue(out DialogRequest? request);

        event Action? Changed;
    }

}
