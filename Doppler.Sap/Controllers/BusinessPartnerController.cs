using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Doppler.Sap.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BusinessPartnerController
    {
        private readonly ILogger<BillingController> _logger;
        private readonly IBusinessPartnerService _businessPartnerService;

        public BusinessPartnerController(ILogger<BillingController> logger, IBusinessPartnerService billingService) =>
            (_logger, _businessPartnerService) = (logger, billingService);

        [HttpPost("CreateOrUpdateBusinessPartner")]
        public async Task<IActionResult> CreateOrUpdateBusinessPartner([FromBody] DopplerUserDTO dopplerUser)
        {
            _logger.LogInformation($"Received user: {dopplerUser.Email}");
            var isValidUser = VerifyUserInformation(dopplerUser);
            if (!string.IsNullOrEmpty(isValidUser))
            {
                return new BadRequestObjectResult(isValidUser);
            }

            try
            {
                await _businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser);

                return new OkResult();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed at mapping data from user: {dopplerUser.Id}, Object sent: {JsonConvert.SerializeObject(dopplerUser)} ", e);
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    ErrorMessage = $"Failed at mapping data from user: {dopplerUser.Id}",
                    ExceptionLogged = e
                });
            }
        }

        private string VerifyUserInformation(DopplerUserDTO dopplerUser)
        {
            if (dopplerUser.BillingCountryCode != "AR")
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it's not from AR");
                return "Invalid billing country value.";
            }
            if (String.IsNullOrEmpty(dopplerUser.FederalTaxID))
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a cuit value");
                return "Invalid cuit value.";
            }
            if (!dopplerUser.planType.HasValue)
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a plan type id");
                return "Invalid plan type value.";
            }
            return string.Empty;
        }
    }
}
