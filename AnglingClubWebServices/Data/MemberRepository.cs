using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class MemberRepository : RepositoryBase, IMemberRepository
    {
        private const string IdPrefix = "Member";
        private readonly ILogger<MemberRepository> _logger;


        public MemberRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MemberRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get;  }

        public async Task AddOrUpdateMember(Member member)
        {
            var client = GetClient();

            if (!member.AllowNameToBeUsed)
            {
                member.Name = "Anonymous";
            }

            if (member.IsNewItem)
            {
                member.DbKey = member.GenerateDbKey(IdPrefix);
            }


            // Check that there aren't already members of this membership number in each active season
            foreach (var season in member.SeasonsActive)
            {
                if (GetMembers().Result.Any(x => x.MembershipNumber == member.MembershipNumber && x.SeasonsActive.Contains(season) && x.DbKey != member.DbKey))
                {
                    throw new Exception($"Membership number {member.MembershipNumber} is already active for {season.SeasonName()}");
                }
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Name", Value = member.Name, Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = member.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Admin", Value = member.Admin ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "Pin", Value = member.Pin, Replace = true },
                new ReplaceableAttribute { Name = "AllowNameToBeUsed", Value = member.AllowNameToBeUsed ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "PreferencesLastUpdated", Value = dateToString(member.PreferencesLastUpdated), Replace = true },
                new ReplaceableAttribute { Name = "PinResetRequired", Value = member.PinResetRequired ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "PinResetRequested", Value = member.PinResetRequested ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "FailedLoginAttempts", Value = member.FailedLoginAttempts.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "LastLoginFailure", Value = dateToString(member.LastLoginFailure), Replace = true },
                new ReplaceableAttribute { Name = "ReLoginRequired", Value = member.ReLoginRequired ? "1" : "0", Replace = true },
            };

            foreach (var season in member.SeasonsActive)
            {
                attributes.Add(new ReplaceableAttribute { Name = "SeasonsActive", Value = ((int)season).ToString(), Replace = true });
            }

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = member.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"Member added: {member.DbKey} - {member.Name}");

            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Member>> GetMembers(Season? activeSeason = null)
        {
            _logger.LogWarning($"Getting members at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var members = new List<Member>();

            var items = await GetData(IdPrefix, "AND Name > ''", "ORDER BY Name");

            foreach (var item in items)
            {
                var member = new Member();

                member.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Name":
                            member.Name = attribute.Value;
                            break;

                        case "MembershipNumber":
                            member.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "Admin":
                            member.Admin = attribute.Value == "0" ? false : true;
                            break;

                        case "SeasonsActive":
                            member.SeasonsActive.Add((Season)Convert.ToInt32(attribute.Value));
                            break;

                        case "Pin":
                            member.Pin = attribute.Value;
                            break;

                        case "PinResetRequired":
                            member.PinResetRequired = attribute.Value == "0" ? false : true; ;
                            break;

                        case "PinResetRequested":
                            member.PinResetRequested = attribute.Value == "0" ? false : true; ;
                            break;
                            
                        case "AllowNameToBeUsed":
                            member.AllowNameToBeUsed = attribute.Value == "1";
                            break;

                        case "PreferencesLastUpdated":
                            member.PreferencesLastUpdated = DateTime.Parse(attribute.Value);
                            break;

                        case "FailedLoginAttempts":
                            member.FailedLoginAttempts = Convert.ToInt32(attribute.Value);
                            break;

                        case "LastLoginFailure":
                            member.LastLoginFailure = DateTime.Parse(attribute.Value);
                            break;

                        case "ReLoginRequired":
                            member.ReLoginRequired = attribute.Value == "0" ? false : true;
                            break;

                        default:
                            break;
                    }
                }

                members.Add(member);
            }

            if (activeSeason.HasValue)
            {
                return members.Where(x => x.SeasonsActive.Contains(activeSeason.Value)).ToList();
            }
            else
            {
                return members;
            }

        }


    }
}
