using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Utils;
using Utils.Validation;

namespace BlogPosts
{
  public class PlaceholderFunction
  {
    private readonly AppInsightsLogger _appLogger;
    private readonly IAPIKeyValidator _apiKeyValidator;
    public PlaceholderFunction(AppInsightsLogger appLogger, IAPIKeyValidator apiKeyValidator)
    {
      _appLogger = appLogger;
      _apiKeyValidator = apiKeyValidator;
    }

    [Function("Ping")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
      _appLogger.LogInformation("Ping function triggered.");

      // This is a simple ping function, no API key validation needed for now
      // var apiKey = req.Headers.GetValues("X-API-Key");

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
      response.WriteString("OK");

      return response;
    }
  }
}
