using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using Utils;
using Utils.Validation;

namespace BlogPosts.Functions
{
    public class PingFunction
    {
        private readonly IAppInsightsLogger<PingFunction> _appLogger;
        private readonly IAPIKeyValidator _apiKeyValidator;
        
        public PingFunction(IAppInsightsLogger<PingFunction> appLogger, IAPIKeyValidator apiKeyValidator)
        {
            _appLogger = appLogger;
            _apiKeyValidator = apiKeyValidator;
        }

        [Function("Ping")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _appLogger.LogInformation("Ping function triggered.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString("{\"status\": \"OK\", \"message\": \"PaaS Platform is running\", \"timestamp\": \"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"}");

            return response;
        }
    }
}
