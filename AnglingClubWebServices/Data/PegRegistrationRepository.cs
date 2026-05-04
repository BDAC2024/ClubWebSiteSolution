using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class PegRegistrationRepository : RepositoryBase, IPegRegistrationRepository
    {
        private const string IdPrefix = "PegRegistration";
        private readonly ILogger<PegRegistrationRepository> _logger;

        public PegRegistrationRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PegRegistrationRepository>();
        }

        public async Task AddOrUpdatePegRegistration(PegRegistration registration)
        {
            if (registration == null)
            {
                throw new AppValidationException("Peg registration payload is required.");
            }

            if (string.IsNullOrWhiteSpace(registration.Stretch) || string.IsNullOrWhiteSpace(registration.Peg))
            {
                throw new AppValidationException("Stretch and Peg are required for peg registration.");
            }

            if (registration.MembershipNumber <= 0)
            {
                throw new AppValidationException("MembershipNumber is required for peg registration.");
            }

            var allRegistrations = await GetPegRegistrations();
            var normalizedStretch = registration.Stretch.Trim();
            var normalizedPeg = registration.Peg.Trim();

            var existing = allRegistrations.SingleOrDefault(x =>
                string.Equals(x.Stretch.Trim(), normalizedStretch, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.Peg.Trim(), normalizedPeg, StringComparison.InvariantCultureIgnoreCase)
                && x.Season == registration.Season
                && x.MembershipNumber == registration.MembershipNumber);

            if (existing != null)
            {
                registration.DbKey = existing.DbKey;
                if (registration.DateRegistered == default)
                {
                    registration.DateRegistered = existing.DateRegistered;
                }
            }

            if (registration.DateRegistered == default)
            {
                registration.DateRegistered = DateTime.UtcNow;
            }

            if (registration.IsNewItem)
            {
                registration.DbKey = registration.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Stretch", Value = normalizedStretch, Replace = true },
                new ReplaceableAttribute { Name = "Peg", Value = normalizedPeg, Replace = true },
                new ReplaceableAttribute { Name = "Season", Value = ((int)registration.Season).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = registration.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "DateRegistered", Value = dateToStorageString(registration.DateRegistered), Replace = true }
            };

            base.SetupTableAttribues(request, registration.DbKey, attributes);

            try
            {
                await WriteInBatches(request, GetClient());
                _logger.LogDebug($"Peg registration saved: {registration.DbKey}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        public async Task<List<PegRegistration>> GetPegRegistrations()
        {
            _logger.LogDebug("Getting peg registrations");

            var registrations = new List<PegRegistration>();
            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var registration = new PegRegistration();
                registration.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Stretch":
                            registration.Stretch = attribute.Value;
                            break;
                        case "Peg":
                            registration.Peg = attribute.Value;
                            break;
                        case "Season":
                            registration.Season = (Season)Convert.ToInt32(attribute.Value);
                            break;
                        case "MembershipNumber":
                            registration.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;
                        case "DateRegistered":
                            registration.DateRegistered = dateFromStorageString(attribute.Value);
                            break;
                        default:
                            break;
                    }
                }

                registrations.Add(registration);
            }

            return registrations;
        }

        public async Task DeletePegRegistration(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new AppValidationException("Id is required to delete a peg registration.");
            }

            var client = GetClient();
            var request = new DeleteAttributesRequest
            {
                DomainName = Domain,
                ItemName = id
            };

            try
            {
                await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }
    }
}
