using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;


//------------------------V
public class TimingHandler : DelegatingHandler
{
    private readonly ILogger<TimingHandler> _logger;

    public TimingHandler(ILogger<TimingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Starting request");

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation($"Finished request in {sw.ElapsedMilliseconds}ms");

        return response;
    }
}
//------------------------Ʌ

//--------------------------------V
public class ValidateHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("X-API-KEY"))
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("You must supply an API key header called X-API-KEY")
            };
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
//--------------------------------Ʌ
