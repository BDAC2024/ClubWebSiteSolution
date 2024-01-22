using AnglingClubWebServices.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace AnglingClubWebServices.Services
{
    public class HealthService : IHealthService
    {
        #region Backing Fields

        private readonly ILogger _logger;

        private readonly IEventRepository _eventRepository;

        #endregion

        #region Constructors

        public HealthService(
            IEventRepository eventRepository,
            ILoggerFactory loggerFactory)
        {
            _eventRepository = eventRepository;

            _logger = loggerFactory.CreateLogger<HealthService>();
        }

        #endregion

        #region Methods

        public void CheckHealth()
        {
            try
            {
                var events = _eventRepository.GetEvents().Result;
                _logger.LogDebug($"Got {events.Count} events from DB");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health Check failed");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        #endregion

    }
}
