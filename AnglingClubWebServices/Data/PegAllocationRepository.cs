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
    public class PegAllocationRepository : RepositoryBase, IPegAllocationRepository
    {
        private const string IdPrefix = "PegAllocation";
        private readonly ILogger<PegAllocationRepository> _logger;

        public PegAllocationRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PegAllocationRepository>();
        }

        public async Task AddOrUpdatePegAllocation(PegAllocation allocation)
        {
            if (allocation == null)
            {
                throw new AppValidationException("Peg allocation payload is required.");
            }

            if (string.IsNullOrWhiteSpace(allocation.Stretch) || string.IsNullOrWhiteSpace(allocation.Peg))
            {
                throw new AppValidationException("Stretch and Peg are required for peg allocation.");
            }

            if (allocation.MembershipNumber <= 0)
            {
                throw new AppValidationException("MembershipNumber is required for peg allocation.");
            }

            if (allocation.DateAllocated == default)
            {
                throw new AppValidationException("DateAllocated is required for peg allocation.");
            }

            var allAllocations = await GetPegAllocations();
            var normalizedStretch = allocation.Stretch.Trim();
            var normalizedPeg = allocation.Peg.Trim();

            var existing = allAllocations.SingleOrDefault(x =>
                string.Equals(x.Stretch.Trim(), normalizedStretch, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.Peg.Trim(), normalizedPeg, StringComparison.InvariantCultureIgnoreCase)
                && x.DateAllocated == allocation.DateAllocated);

            if (existing != null)
            {
                allocation.DbKey = existing.DbKey;
                allocation.Season = existing.Season;
            }

            if (allocation.Season == default)
            {
                var seasonForDate = EnumUtils.SeasonForDate(allocation.DateAllocated.ToDateTime(TimeOnly.MinValue));
                if (!seasonForDate.HasValue)
                {
                    throw new AppValidationException("DateAllocated does not fall within a known season.");
                }
                allocation.Season = seasonForDate.Value;
            }

            if (allocation.IsNewItem)
            {
                allocation.DbKey = allocation.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Stretch", Value = normalizedStretch, Replace = true },
                new ReplaceableAttribute { Name = "Peg", Value = normalizedPeg, Replace = true },
                new ReplaceableAttribute { Name = "Season", Value = ((int)allocation.Season).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = allocation.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "DateAllocated", Value = dateOnlyToString(allocation.DateAllocated), Replace = true }
            };

            base.SetupTableAttribues(request, allocation.DbKey, attributes);

            try
            {
                await WriteInBatches(request, GetClient());
                _logger.LogDebug($"Peg allocation saved: {allocation.DbKey}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        public async Task<List<PegAllocation>> GetPegAllocations()
        {
            _logger.LogDebug("Getting peg allocations");

            var allocations = new List<PegAllocation>();
            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var allocation = new PegAllocation();
                allocation.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Stretch":
                            allocation.Stretch = attribute.Value;
                            break;
                        case "Peg":
                            allocation.Peg = attribute.Value;
                            break;
                        case "Season":
                            allocation.Season = (Season)Convert.ToInt32(attribute.Value);
                            break;
                        case "MembershipNumber":
                            allocation.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;
                        case "DateAllocated":
                            allocation.DateAllocated = dateOnlyFromString(attribute.Value);
                            break;
                        default:
                            break;
                    }
                }

                allocations.Add(allocation);
            }

            return allocations;
        }

        public async Task DeletePegAllocation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new AppValidationException("Id is required to delete a peg allocation.");
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

        private string dateOnlyToString(DateOnly value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private DateOnly dateOnlyFromString(string value)
        {
            return DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
