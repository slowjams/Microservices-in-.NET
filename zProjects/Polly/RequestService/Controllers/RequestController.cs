using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RequestService.Polices;

namespace RequestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        //
        private readonly IHttpClientFactory _httpClientFactory;

        public RequestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        //


        //
        private readonly MyGitHubClient _gitHubClient;

        public RequestController(MyGitHubClient gitGitHubClient)
        {
            _gitHubClient = gitGitHubClient;
        }
        //

        [HttpGet]
        public async Task<ActionResult> GetUsingNameClient()
        {
            var client = _httpClientFactory.CreateClient("GitHubClient");
            var result = await client.GetStringAsync("/");

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetUsingTypeClient()
        {
            var result = await _gitHubClient.Client.GetStringAsync("/");
            return Ok(result);
        }
    }

    public class MyGitHubClient
    {
        public MyGitHubClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class RequestController2 : ControllerBase  // inject policy directly
    {
        private readonly ClientPolicy _clientPolicy;

        public RequestController2(ClientPolicy clientPolicy)
        {
            _clientPolicy = clientPolicy;

        }

        public async Task<ActionResult> MakeRequest()
        {
            var client = new HttpClient();

            var response = await client.GetAsync("http://localhost:5000/api/response/25");  

            var responseRetry = await _clientPolicy.ExponentialHttpRetry.ExecuteAsync(() =>
            {
                return client.GetAsync("http://localhost:5000/api/response/25");
            });

            // 
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var task = _clientPolicy.TimeoutPolicy.ExecuteAsync((ct) => SomeMethodAsync(ct), cancellationToken);
            try
            {
                await task;
            }
            catch (OperationCanceledException exception)   // cancellationTokenSource.Cancel() called from outside
            {
                // ...
            }
            //



            if (response.IsSuccessStatusCode)
            {
                Console.Write("--> ResonseService returned a Success");
                return Ok();
            }
            else
            {
                Console.Write("--> ResonseService returned a FAILURE");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<int> SomeMethodAsync(CancellationToken ct = default)
        {
            Console.WriteLine($"{nameof(SomeMethodAsync)} has been called.");
            await Task.Delay(15000, ct); //It is aware of the CancellationToken
            return 1;
        }
    }
}