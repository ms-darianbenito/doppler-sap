using Doppler.Sap.Enums;
using Doppler.Sap.Mappers;
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
            _logger.LogInformation(String.Format("Received user: {0}", dopplerUser.Email));
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
                _logger.LogError(String.Format("Failed at mapping data from user: {0}, Object sent: {1} ", dopplerUser.Id, JsonConvert.SerializeObject(dopplerUser)), e);
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    ErrorMessage = String.Format("Failed at mapping data from user: {0}", dopplerUser.Id, JsonConvert.SerializeObject(dopplerUser)),
                    ExceptionLogged = e
                });
            }
        }

        private string VerifyUserInformation(DopplerUserDTO dopplerUser)
        {
            if (dopplerUser.BillingCountryCode != "AR")
            {
                _logger.LogInformation(String.Format("{0} won't be sent to SAP because it's not from AR", dopplerUser.Email));
                return "Invalid billing country value.";
            }
            if (String.IsNullOrEmpty(dopplerUser.FederalTaxID))
            {
                _logger.LogInformation(String.Format("{0} won't be sent to SAP because it doesn't have a cuit value", dopplerUser.Email));
                return "Invalid cuit value.";
            }
            if (!dopplerUser.planType.HasValue)
            {
                _logger.LogInformation(String.Format("{0} won't be sent to SAP because it doesn't have a plan type id", dopplerUser.Email));
                return "Invalid plan type value.";
            }
            return string.Empty;
        }
    }
}
