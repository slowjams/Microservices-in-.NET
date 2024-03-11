using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace ServiceB.Controllers;

[ApiController]
[Route("")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;
    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("ping-internal")]
    public async Task<IActionResult> PingInternalAsync()
    {
        using (var activity = CustomTraces.Default.StartActivity("BuildPingResult"))
        {
            var baggage = activity?.GetBaggageItem("sample.proxy");
            activity?.SetTag("http.method", Request.Method);
            activity?.SetTag("http.url", Request.Path);
            activity?.SetTag("http.host", Request.Host.Value);
            activity?.SetTag("http.scheme", Request.Scheme);

            var random = new Random().Next(50, 100);
            await Task.Delay(random);
            activity?.SetTag("serviceb.delay", random);
        }
        return Ok(
            new { Status = "ok" }
        );
    }
}