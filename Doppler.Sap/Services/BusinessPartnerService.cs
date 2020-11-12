using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly IQueuingService _queuingService;
        private readonly ILogger<BusinessPartnerService> _logger;
        private readonly SapConfig _sapConfig;

        public BusinessPartnerService(
            IQueuingService queuingService,
            ILogger<BusinessPartnerService> logger,
            IOptions<SapConfig> sapConfig)
        {
            _queuingService = queuingService;
            _logger = logger;
            _sapConfig = sapConfig.Value;
        }

        public Task CreateOrUpdateBusinessPartner(DopplerUserDto dopplerUser)
        {
            var userVerificationError = VerifyUserInformation(dopplerUser);
            if (!string.IsNullOrEmpty(userVerificationError))
            {
                throw new ValidationException(userVerificationError);
            }

            _logger.LogInformation($"Add to Task in Queue: {SapTaskEnum.CreateOrUpdateBusinessPartner}");
            _queuingService.AddToTaskQueue(
                new SapTask()
                {
                    TaskType = SapTaskEnum.CreateOrUpdateBusinessPartner,
                    DopplerUser = dopplerUser
                }
            );

            return Task.CompletedTask;
        }


        private string VerifyUserInformation(DopplerUserDto dopplerUser)
        {
            if (!_sapConfig.SapServiceConfigsByCountryCode.ContainsKey(dopplerUser.BillingCountryCode))
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it's not from {string.Join(", ", _sapConfig.SapServiceConfigsByCountryCode.Select(x => x.Key))}");
                return "Invalid billing country value.";
            }
            if (String.IsNullOrEmpty(dopplerUser.FederalTaxID))
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a cuit value");
                return "Invalid cuit value.";
            }
            if (!dopplerUser.PlanType.HasValue)
            {
                _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a plan type id");
                return "Invalid plan type value.";
            }

            return string.Empty;
        }
    }
}
