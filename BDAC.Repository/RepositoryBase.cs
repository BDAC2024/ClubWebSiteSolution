using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDAC.Repository
{
    public abstract class RepositoryBase
    {
        private readonly string _assessId;
        private readonly string _secret;
        private readonly string _domain;
        private ILogger<RepositoryBase> _logger;

        public RepositoryBase(string accessId, string secret, string domain, ILoggerFactory loggerFactory)
        {
            _assessId = accessId;
            _secret = secret;
            _domain = domain;
            _logger = loggerFactory.CreateLogger<RepositoryBase>();

            if (!checkDomainExists(_domain).Result)
            {
                createDomain(_domain).Wait();
            }

        }

        internal string Domain { get { return _domain; } }

        private async Task createDomain(string domainName)
        {
            _logger.LogDebug($"Creating domain {domainName}");

            var client = GetClient();

            CreateDomainRequest request = new CreateDomainRequest(domainName);

            CreateDomainResponse response = await client.CreateDomainAsync(request);

            _logger.LogDebug("createDomain returned");
            _logger.LogDebug(response.ToString());
            
        }

        private async Task<bool> checkDomainExists(string domainName)
        {
            _logger.LogDebug($"Checking for domain   {domainName}");

            var client = GetClient();

            ListDomainsRequest request = new ListDomainsRequest();

            ListDomainsResponse response = await client.ListDomainsAsync(request);

            _logger.LogDebug(response.ToString());

            return response.DomainNames.Any(n => n == domainName);
        }

        internal AmazonSimpleDBClient GetClient()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient(_assessId, _secret, Amazon.RegionEndpoint.EUWest1);

            return client;
        }

    }
}
