using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Day04EventNotifier;

public sealed class NotifyProcessedFile
{
    private readonly ILogger<NotifyProcessedFile> _logger;

    public NotifyProcessedFile(ILogger<NotifyProcessedFile> logger) => _logger = logger;

    [Function("NotifyProcessedFile")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "NotifyProcessedFile")]
        HttpRequest request)
    {
        FileNotification? payload;
        try
        {
            payload = JsonSerializer.Deserialize<FileNotification>(request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(new { error = "Invalid JSON payload" });
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.FileName))
            return new BadRequestObjectResult(new { error = "fileName is required" });

        _logger.LogInformation("Received file notification: {FileName}; eventType={EventType}",
            payload.FileName, payload.EventType ?? "unspecified");

        return new OkObjectResult(new
        {
            status = "accepted",
            fileName = payload.FileName,
            eventType = payload.EventType,
            receivedAtUtc = DateTimeOffset.UtcNow
        });
    }
}

public sealed class FileNotification
{
    public string? FileName { get; set; }
    public string? EventType { get; set; }
}
