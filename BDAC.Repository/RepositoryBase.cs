using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDAC.Repository
{
    public abstract class RepositoryBase
    {
        internal const string DOMAIN = "Tester";
        //internal const string DOMAIN = "AnglingClub";

        public RepositoryBase()
        {
            if (!checkDomainExists(DOMAIN).Result)
            {
                createDomain(DOMAIN).Wait();
            }

        }

        private async Task createDomain(string domainName)
        {
            Console.WriteLine($"Creating domain {domainName}");

            var client = GetClient();

            CreateDomainRequest request = new CreateDomainRequest(domainName);

            CreateDomainResponse response = await client.CreateDomainAsync(request);

            Console.WriteLine("createDomain returned");
            Console.WriteLine(response);
            Console.ReadLine();
        }

        private async Task<bool> checkDomainExists(string domainName)
        {
            Console.WriteLine($"Checking for domain   {domainName}");

            var client = GetClient();

            ListDomainsRequest request = new ListDomainsRequest();

            ListDomainsResponse response = await client.ListDomainsAsync(request);

            Console.WriteLine(response);

            return response.DomainNames.Any(n => n == domainName);
        }

        internal AmazonSimpleDBClient GetClient()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient("xxx", "yyy", Amazon.RegionEndpoint.EUWest1);

            return client;
        }

    }
}
