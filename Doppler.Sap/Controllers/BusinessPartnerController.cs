using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BusinessPartnerController
    {
        private readonly ILogger<BusinessPartnerController> _logger;
        private readonly IBusinessPartnerService _businessPartnerService;

        public BusinessPartnerController(ILogger<BusinessPartnerController> logger, IBusinessPartnerService businessPartnerService)
        {
            _logger = logger;
            _businessPartnerService = businessPartnerService;
        }

        [HttpPost("CreateOrUpdateBusinessPartner")]
        public async Task<IActionResult> CreateOrUpdateBusinessPartner([FromBody] DopplerUserDto dopplerUser)
        {
            _logger.LogInformation($"Received user: {dopplerUser.Email}");

            try
            {
                await _businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser);

                return new OkObjectResult("Successfully");
            }
            catch (ValidationException e)
            {
                _logger.LogError(e, $"Failed at creating/updating user: {dopplerUser.Id}. Because the user has a validation error: {e.Message}");
                return new BadRequestObjectResult(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed at creating/updating user: {dopplerUser.Id}, Object sent: {JsonConvert.SerializeObject(dopplerUser)} ");
                return new ObjectResult(new
                {
                    StatusCode = 400,
                    ErrorMessage = $"Failed at creating/updating user: {dopplerUser.Id}",
                    ExceptionLogged = e
                });
            }
        }
    }
}
