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
    private readonly ILogger<PlaceholderFunction> _logger;
    private readonly IAPIKeyValidator _apiKeyValidator;
    public PlaceholderFunction(ILogger<PlaceholderFunction> logger, IAPIKeyValidator apiKeyValidator)
    {
      _logger = logger;
      _apiKeyValidator = apiKeyValidator;
    }

    [Function("Ping")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
      _logger.LogInformation("Ping function triggered.");

      // var apiKey = req.Headers.GetValues("X-API-Key");

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
      response.WriteString("OK");

      return response;
    }
  }
}
