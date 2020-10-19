using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace Doppler.Sap
{
    public class TaskRepeater : IHostedService
    {
        private readonly ILogger<TaskRepeater> _logger;
        private readonly IQueuingService _queuingService;
        private readonly ISapService _sapService;

        public TaskRepeater(ILogger<TaskRepeater> logger, IQueuingService queuingService, ISapService sapService)
        {
            _logger = logger;
            _queuingService = queuingService;
            _sapService = sapService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service.");

            try
            {
                var handler = new HttpClientHandler
                {
                    //ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; },
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    UseCookies = false
                };

                var client = new HttpClient(handler);
                var sapResponse = await client.SendAsync(new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://172.25.16.86:50000/b1s/v1/Login/"),
                    Content = new StringContent(JsonConvert.SerializeObject(
                            new SapConfig
                            {
                                CompanyDB = "MK_ARG_TEST",
                                Password = "RnbEh%3SAc",
                                UserName = "interfaz"
                            }),
                        Encoding.UTF8,
                        "application/json"),
                    Method = HttpMethod.Post
                });
                sapResponse.EnsureSuccessStatusCode();
                Log.Information("[TEST]connect correctly");
            }
            catch (Exception e)
            {
                Log.Error(e, "[TEST]Error starting session in Sap." + e);
            }
            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    var dequeuedTask = _queuingService.GetFromTaskQueue();

            //    if (dequeuedTask != null)
            //    {
            //        try
            //        {
            //            var sapServiceResponse = await _sapService.SendToSap(dequeuedTask);
            //            if (sapServiceResponse.IsSuccessful)
            //            {
            //                _logger.LogInformation($"Succeeded at {sapServiceResponse.TaskName}.");
            //            }
            //            else
            //            {
            //                _logger.LogError(
            //                    $"Failed at {sapServiceResponse.TaskName}, SAP response was {sapServiceResponse.SapResponseContent}");
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            _logger.LogError($"Unexpected error while sending data to Sap exception: {e.StackTrace}");
            //        }
            //    }
            //    else
            //    {
            //        await Task.Delay(3000, cancellationToken);
            //    }
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("NormalHostedService stopped.");
            return Task.CompletedTask;
        }
    }
}
