using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SharedStorage.Services;
using System.Net;

namespace Functions.BlogPosts.Functions;

public class QueueMessageFunction
{
    private readonly ILogger<QueueMessageFunction> _logger;
    private readonly IQueueStorageService _queueService;

    public QueueMessageFunction(ILogger<QueueMessageFunction> logger, IQueueStorageService queueService)
    {
        _logger = logger;
        _queueService = queueService;
    }

    [Function("SendQueueMessage")]
    public async Task<HttpResponseData> SendQueueMessage(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("SendQueueMessage function processed a request.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var queueName = req.Query["queue"] ?? "notifications";
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body cannot be empty.");
                return badResponse;
            }

            var result = await _queueService.SendMessageAsync(queueName, requestBody);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                messageId = result.MessageId,
                queueName = queueName,
                message = "Message sent successfully"
            });
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending queue message");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while sending the message.");
            return errorResponse;
        }
    }

    [Function("ReceiveQueueMessages")]
    public async Task<HttpResponseData> ReceiveQueueMessages(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("ReceiveQueueMessages function processed a request.");

        try
        {
            var queueName = req.Query["queue"] ?? "notifications";
            var maxMessages = int.TryParse(req.Query["maxMessages"], out var max) ? max : 10;
            
            var messages = await _queueService.ReceiveMessagesAsync(queueName, maxMessages);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                queueName = queueName,
                messageCount = messages.Count(),
                messages = messages.Select(m => new
                {
                    messageId = m.MessageId,
                    popReceipt = m.PopReceipt,
                    messageText = m.MessageText,
                    dequeueCount = m.DequeueCount,
                    insertedOn = m.InsertedOn,
                    expiresOn = m.ExpiresOn
                })
            });
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving queue messages");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while receiving messages.");
            return errorResponse;
        }
    }

    [Function("GetQueueInfo")]
    public async Task<HttpResponseData> GetQueueInfo(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetQueueInfo function processed a request.");

        try
        {
            var queueName = req.Query["queue"] ?? "notifications";
            
            var length = await _queueService.GetQueueLengthAsync(queueName);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                queueName = queueName,
                approximateMessageCount = length,
                message = "Queue information retrieved successfully"
            });
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue info");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while getting queue information.");
            return errorResponse;
        }
    }
}