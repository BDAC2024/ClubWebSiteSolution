using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AnglingClubWebServices
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// AnglingClubWebServices::AnglingClubWebServices.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :

        // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
        // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
        //
        // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
        // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        // 
        // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
        // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
        }

        private static readonly DateTime LambdaTime = DateTime.Now;

        // Note that this can be extracted by looking in cloudwatch after calling the api/healtcheck from postman and copying the response from there
        private string realReq = @"
        {
            'Resource': '/{proxy+}',
            'Path': '/api/healthcheck',
            'HttpMethod': 'GET',
            'Headers': {
                'Accept': '*/*',
                'Accept-Encoding': 'gzip, deflate, br',
                'Cache-Control': 'no-cache',
                'CloudFront-Forwarded-Proto': 'https',
                'CloudFront-Is-Desktop-Viewer': 'true',
                'CloudFront-Is-Mobile-Viewer': 'false',
                'CloudFront-Is-SmartTV-Viewer': 'false',
                'CloudFront-Is-Tablet-Viewer': 'false',
                'CloudFront-Viewer-Country': 'GB',
                'Host': 't5nynu5k43.execute-api.eu-west-1.amazonaws.com',
                'Postman-Token': '9b5ee25b-95ad-4a89-93ef-711fabbda104',
                'User-Agent': 'PostmanRuntime/7.28.0',
                'Via': '1.1 1064639c622430d6b0382968293fd56e.cloudfront.net (CloudFront)',
                'X-Amz-Cf-Id': '-hwcXYIM6mTExEuibdTST-S2nOe_FVKZQWVM32_p4u8OxqW7nj8h_g==',
                'X-Amzn-Trace-Id': 'Root=1-6092cfea-75dc6e145a8d260d55b45e61',
                'X-Forwarded-For': '81.153.217.128, 130.176.96.147',
                'X-Forwarded-Port': '443',
                'X-Forwarded-Proto': 'https'
            },
            'MultiValueHeaders': {
                'Accept': [
                    '*/*'
                ],
                'Accept-Encoding': [
                    'gzip, deflate, br'
                ],
                'Cache-Control': [
                    'no-cache'
                ],
                'CloudFront-Forwarded-Proto': [
                    'https'
                ],
                'CloudFront-Is-Desktop-Viewer': [
                    'true'
                ],
                'CloudFront-Is-Mobile-Viewer': [
                    'false'
                ],
                'CloudFront-Is-SmartTV-Viewer': [
                    'false'
                ],
                'CloudFront-Is-Tablet-Viewer': [
                    'false'
                ],
                'CloudFront-Viewer-Country': [
                    'GB'
                ],
                'Host': [
                    't5nynu5k43.execute-api.eu-west-1.amazonaws.com'
                ],
                'Postman-Token': [
                    '9b5ee25b-95ad-4a89-93ef-711fabbda104'
                ],
                'User-Agent': [
                    'PostmanRuntime/7.28.0'
                ],
                'Via': [
                    '1.1 1064639c622430d6b0382968293fd56e.cloudfront.net (CloudFront)'
                ],
                'X-Amz-Cf-Id': [
                    '-hwcXYIM6mTExEuibdTST-S2nOe_FVKZQWVM32_p4u8OxqW7nj8h_g=='
                ],
                'X-Amzn-Trace-Id': [
                    'Root=1-6092cfea-75dc6e145a8d260d55b45e61'
                ],
                'X-Forwarded-For': [
                    '81.153.217.128, 130.176.96.147'
                ],
                'X-Forwarded-Port': [
                    '443'
                ],
                'X-Forwarded-Proto': [
                    'https'
                ]
            },
            'QueryStringParameters': null,
            'MultiValueQueryStringParameters': null,
            'PathParameters': {
                'proxy': 'api/healthcheck'
            },
            'StageVariables': null,
            'RequestContext': {
                'Path': '/Prod/api/healthcheck',
                'AccountId': '406726073663',
                'ResourceId': 'mr94pg',
                'Stage': 'Prod',
                'RequestId': 'e530e988-3e35-4fbf-b8cc-5890dc691271',
                'Identity': {
                    'CognitoIdentityPoolId': null,
                    'AccountId': null,
                    'CognitoIdentityId': null,
                    'Caller': null,
                    'ApiKey': null,
                    'ApiKeyId': null,
                    'AccessKey': null,
                    'SourceIp': '81.153.217.128',
                    'CognitoAuthenticationType': null,
                    'CognitoAuthenticationProvider': null,
                    'UserArn': null,
                    'UserAgent': 'PostmanRuntime/7.28.0',
                    'User': null,
                    'ClientCert': null
                },
                'ResourcePath': '/{proxy+}',
                'HttpMethod': 'GET',
                'ApiId': 't5nynu5k43',
                'ExtendedRequestId': 'e3VspHnwjoEFcJQ=',
                'ConnectionId': null,
                'ConnectionAt': 0,
                'DomainName': 't5nynu5k43.execute-api.eu-west-1.amazonaws.com',
                'DomainPrefix': 't5nynu5k43',
                'EventType': null,
                'MessageId': null,
                'RouteKey': null,
                'Authorizer': null,
                'OperationName': null,
                'Error': null,
                'IntegrationLatency': null,
                'MessageDirection': null,
                'RequestTime': '05/May/2021:17:03:38 +0000',
                'RequestTimeEpoch': 1620234218473,
                'Status': null
            },
            'Body': null,
            'IsBase64Encoded': false
        }";

        public override async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(JsonConvert.SerializeObject(request));

            if (request.Resource == "WarmingLambda")
            {
                Console.WriteLine("Warming in overridden FunctionHandlerAsync…");
                Console.WriteLine($"LambdaTime : {LambdaTime.ToShortTimeString()}");

                var concurrencyCount = 1;
                int.TryParse(request.Body, out concurrencyCount);

                //if (concurrencyCount > 1)
                //{
                //    Console.WriteLine($"Warming instance { concurrencyCount}.");
                //    var client = new AmazonLambdaClient();
                //    await client.InvokeAsync(new Amazon.Lambda.Model.InvokeRequest
                //    {
                //        FunctionName = lambdaContext.FunctionName,
                //        InvocationType = InvocationType.RequestResponse,
                //        Payload = JsonConvert.SerializeObject(new APIGatewayProxyRequest
                //        {
                //            Resource = request.Resource,
                //            Body = (concurrencyCount - 1).ToString()
                //        })
                //    });
                //}


                var apiReq = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(realReq);

                return await base.FunctionHandlerAsync(apiReq, lambdaContext);
                //return new APIGatewayProxyResponse { };


            }

            return await base.FunctionHandlerAsync(request, lambdaContext);
        }
    }
}
