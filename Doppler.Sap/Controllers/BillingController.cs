using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Doppler.Sap.Controllers
{
    [ApiController]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly ILogger<BillingController> logger;

        public BillingController(ILogger<BillingController> logger)
        {
            this.logger = logger;
        }

        [HttpPost("SetCurrencyRate")]
        [SwaggerOperation(Summary = "Set currency rate in SAP")]
        [SwaggerResponse(200, "The operation was successfully")]
        [SwaggerResponse(400, "The operation failed")]
        public IActionResult SetCurrencyRate()
        {
            return Ok("Works fine!!");
        }
    }
}
