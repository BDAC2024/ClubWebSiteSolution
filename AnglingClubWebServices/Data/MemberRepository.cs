using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class MemberRepository : RepositoryBase, IMemberRepository
    {
        private const string IdPrefix = "Member";
        private readonly ILogger<MemberRepository> _logger;

        public MemberRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value.AWSAccessId, opts.Value.AWSSecret, opts.Value.SimpleDbDomain, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MemberRepository>();
        }

        public async Task AddOrUpdateMember(Member member)
        {
            var client = GetClient();

            if (member.IsNewItem)
            {
                member.DbKey = member.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Name", Value = member.Name, Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = member.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Admin", Value = member.Admin ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "LastPaid", Value = dateToString(member.LastPaid), Replace = true },
                new ReplaceableAttribute { Name = "Enabled", Value = member.Enabled ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "Pin", Value = member.Pin.ToString(), Replace = true },

            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = member.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                _logger.LogDebug($"Member added: {member.DbKey} - {member.Name}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Member>> GetMembers()
        {
            _logger.LogWarning($"Getting members at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var members = new List<Member>();

            var client = GetClient();

            SelectRequest request = new SelectRequest();
            request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{IdPrefix}:%' AND Name > '' ORDER BY Name";

            SelectResponse response = await client.SelectAsync(request);

            foreach (var item in response.Items)
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

                        case "LastPaid":
                            member.LastPaid = DateTime.Parse(attribute.Value);
                            break;

                        case "Enabled":
                            member.Enabled = attribute.Value == "0" ? false : true;
                            break;

                        case "Pin":
                            member.Pin = Convert.ToInt32(attribute.Value);
                            break;

                        default:
                            break;
                    }
                }

                members.Add(member);
            }

            return members;

        }


    }
}
