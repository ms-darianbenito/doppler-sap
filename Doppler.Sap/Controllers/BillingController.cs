using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Doppler.Sap.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BillingController
    {
        private readonly ILogger<BillingController> _logger;
        private readonly IBillingService _billingService;

        public BillingController(ILogger<BillingController> logger, IBillingService billingService) =>
            (_logger, _billingService) = (logger, billingService);

        [HttpPost("SetCurrencyRate")]
        [SwaggerOperation(Summary = "Set currency rate in SAP")]
        [SwaggerResponse(200, "The operation was successfully")]
        [SwaggerResponse(400, "The operation failed")]
        public async Task<IActionResult> SetCurrencyRate([FromBody] List<CurrencyRateDto> currencyRate)
        {
            _logger.LogDebug("Setting currency date.");

            await _billingService.SendCurrencyToSap(currencyRate);

            return new OkObjectResult("Successfully");
        }

        [HttpPost("CreateBillingRequest")]
        public async Task<IActionResult> CreateBillingRequest([FromBody] List<BillingRequest> billingRequest)
        {
            _logger.LogDebug("Creating Billing request.");

            await _billingService.CreateBillingRequest(billingRequest);

            return new OkObjectResult("Successfully");
        }

        [HttpPatch("UpdateBilling")]
        public async Task<IActionResult> UpdateBilling([FromBody] UpdateBillingRequest billingRequest)
        {
            _logger.LogDebug("Updating Billing request.");

            await _billingService.UpdateBilling(billingRequest);

            return new OkObjectResult("Successfully");
        }
    }
}
