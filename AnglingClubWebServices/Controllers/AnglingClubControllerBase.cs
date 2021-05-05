using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;


namespace AnglingClubWebServices.Controllers
{
    public class AnglingClubControllerBase : ControllerBase
    {
        private readonly Stopwatch _timer = new Stopwatch();

        public AnglingClubControllerBase()
        {

        }

        #region Properties

        protected ILogger Logger { get; set; } = null;

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
    }
}
