using AnglingClubWebsite.Models;
using System.Collections.Concurrent;

namespace AnglingClubWebsite.Services
{

    public sealed class DialogQueue : IDialogQueue
    {
        private readonly ConcurrentQueue<DialogRequest> _queue = new();

        public event Action? Changed;

        public void Enqueue(DialogRequest request)
        {
            _queue.Enqueue(request);
            Changed?.Invoke();
        }

        public bool TryDequeue(out DialogRequest? request)
            => _queue.TryDequeue(out request);
    }
}



