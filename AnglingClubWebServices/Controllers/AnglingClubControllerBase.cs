using AnglingClubShared.Entities;
using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace AnglingClubWebServices.Controllers
{
    public class AnglingClubControllerBase : ControllerBase
    {
        private readonly Stopwatch _timer = new Stopwatch();
        private Member _currentUser = null;

        public AnglingClubControllerBase()
        {

        }

        #region Properties

        protected ILogger Logger { get; set; } = null;
        protected bool IsProd {
            get
            {
                var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                var url = location.AbsoluteUri;
                return url.ToLower().Contains("amazonaws");
            }
        }

        protected string Caller {
            get
            {
                try
                {
                    Request.Headers.TryGetValue("Origin", out var caller);
                    return caller;
                }
                catch
                {
                    return "CallerUnknown";
                }
            }
        }

        protected Member CurrentUser {
            get
            {
                if (_currentUser == null)
                {
                    await GetCurrentUserAsync(_authService).Result;
                    _currentUser = (Member)HttpContext.Items["User"];
                }

                return _currentUser;
            }
        }

        #endregion

        #region Protected Methods

        protected void StartTimer()
        {
            ValidateLogger();
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                _timer.Start();
            }
        }


        protected void ReportTimer(string caller)
        {
            ValidateLogger();

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                if (_timer.IsRunning)
                {
                    _timer.Stop();
                }
                Logger.LogDebug($"{caller} took {_timer.Elapsed.TotalSeconds.ToString("n2")} seconds");
            }

        }

        #endregion

        #region Helper Methods

        private void ValidateLogger()
        {
            if (Logger == null)
            {
                throw new Exception("Logger has not been defined in API base");
            }
        }

        #endregion

        //private Member? _currentUser;
        private Task<Member>? _currentUserTask;

        protected Task<Member> GetCurrentUserAsync(IAuthService authService)
        {
            if (_currentUser is not null)
            {
                return Task.FromResult(_currentUser);
            }

            if (_currentUserTask is not null)
            {
                return _currentUserTask;
            }

            var key = User.FindFirst("Key")?.Value;
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new UnauthorizedAccessException("Missing Key claim.");
            }

            _currentUserTask = LoadAsync(key, authService);
            return _currentUserTask;

            async Task<Member> LoadAsync(string userKey, IAuthService svc)
            {
                _currentUser = await svc.GetAuthorisedUserByKey(userKey);
                return _currentUser;
            }
        }
    }
}
