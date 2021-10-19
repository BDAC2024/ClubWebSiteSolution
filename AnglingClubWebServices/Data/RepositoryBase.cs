using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public abstract class RepositoryBase
    {
        private readonly string _assessId;
        private readonly string _secret;
        private readonly string _domain;
        private ILogger<RepositoryBase> _logger;

        private const int QUERY_BATCH_SIZE = 2000;
        private const int UPDATE_BATCH_SIZE = 25;
        private int batchNumber = 0;


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

        internal async Task<int> GetNextId()
        {
            var client = GetClient();
            int nextId = -1;

            GetAttributesRequest request = new GetAttributesRequest();
            request.DomainName = _domain;
            request.ItemName = "NextId";
            request.AttributeNames = new List<string> { "Id" };
            request.ConsistentRead = true;

            try
            {
                GetAttributesResponse response = await client.GetAttributesAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.Attributes.Count > 0)
                    {
                        nextId = Convert.ToInt32(response.Attributes[0].Value);
                    }

                    PutAttributesRequest putRequest = new PutAttributesRequest();
                    putRequest.DomainName = _domain;
                    putRequest.ItemName = "NextId";
                    putRequest.Attributes.Add(
                        new ReplaceableAttribute { Name = "Id", Value = numberToString(++nextId), Replace = true }
                    );

                    try
                    {
                        PutAttributesResponse putResponse = await client.PutAttributesAsync(putRequest);
                    }
                    catch (AmazonSimpleDBException ex)
                    {
                        _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                        throw;
                    }
                }

            }
            catch (System.Exception)
            {

                throw;
            }

            return nextId;
        }

        internal AmazonSimpleDBClient GetClient()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient(_assessId, _secret, Amazon.RegionEndpoint.EUWest1);

            return client;
        }

        internal string numberToString(int number)
        {
            var numString = number.ToString("0000000000");

            return numString;
        }

        internal string pointsToString(float number)
        {
            var numString = number.ToString("000.000");

            return numString;
        }

        internal string weightToString(float number)
        {
            var numString = number.ToString("0000.0000");

            return numString;
        }

        internal string dateToString(DateTime date)
        {
            //return date.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        protected async Task<List<Item>> GetData(string idPrefix, string additionalWhereClause = "", string orderByClause = "")
        {
            List<Item> items = new List<Item>();

            var client = GetClient();
            string nextToken = null;

            do
            {
                SelectRequest request = new SelectRequest();
                request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{(idPrefix == "" ? "" : $"{idPrefix}:")}%' {additionalWhereClause} {orderByClause} LIMIT {QUERY_BATCH_SIZE}";
                request.NextToken = nextToken;

                SelectResponse response = await client.SelectAsync(request);
                nextToken = response.NextToken;

                items.AddRange(response.Items);

            } while (nextToken != null);

            return items;
        }

        protected async Task WriteInBatches(BatchPutAttributesRequest request, AmazonSimpleDBClient client)
        {
            List<ReplaceableAttribute> attributes = new List<ReplaceableAttribute>();

            request.Items = request.Items.OrderBy(x => x.Name).ToList();

            while (request.Items.Any())
            {
                batchNumber++;

                BatchPutAttributesRequest requestBatch = new BatchPutAttributesRequest();
                requestBatch.DomainName = request.DomainName;

                requestBatch.Items.AddRange(request.Items.Take(UPDATE_BATCH_SIZE));

                await storeBatchOfItems(client, requestBatch);

                request.Items.RemoveRange(0, request.Items.Count() < UPDATE_BATCH_SIZE ? request.Items.Count() : UPDATE_BATCH_SIZE);
            }
        }

        private async Task storeBatchOfItems(AmazonSimpleDBClient client, BatchPutAttributesRequest request)
        {
            try
            {
                /*
                _logger.LogDebug($"Writing new batch {batchNumber}...");
                var itemNo = 1;

                foreach (var item in request.Items)
                {
                    _logger.LogDebug($"  Item: {itemNo++} = {item.Name}");
                }
                */

                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }


    }
}
