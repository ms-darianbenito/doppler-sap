using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Doppler.Sap.Validations.BusinessPartner;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
        private readonly IEnumerable<IBusinessPartnerValidation> _businessPartnerValidations;

        public BusinessPartnerService(
            IQueuingService queuingService,
            ILogger<BusinessPartnerService> logger,
            IOptions<SapConfig> sapConfig,
            IEnumerable<IBusinessPartnerValidation> businessPartnerValidations)
        {
            _queuingService = queuingService;
            _logger = logger;
            _sapConfig = sapConfig.Value;
            _businessPartnerValidations = businessPartnerValidations;
        }

        public Task CreateOrUpdateBusinessPartner(DopplerUserDto dopplerUser)
        {
            var sapSystem = SapSystemHelper.GetSapSystemByBillingSystem(dopplerUser.BillingSystemId);
            if (!GetValidator(sapSystem).IsValid(dopplerUser, sapSystem, _sapConfig, out var userVerificationError))
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

        private IBusinessPartnerValidation GetValidator(string sapSystem)
        {
            // Check if exists billing validator for the sapSystem
            var validator = _businessPartnerValidations.FirstOrDefault(m => m.CanValidateSapSystem(sapSystem));
            if (validator == null)
            {
                _logger.LogError($"Billing Request won't be sent to SAP because the sapSystem '{sapSystem}' is not supported.");
                throw new ArgumentException(nameof(sapSystem), $"The sapSystem '{sapSystem}' is not supported.");
            }

            return validator;
        }

        //private string VerifyUserInformation(DopplerUserDto dopplerUser)
        //{
        //    var sapSystem = SapSystemHelper.GetSapSystemByBillingSystem(dopplerUser.BillingSystemId);

        //    if (!_sapConfig.SapServiceConfigsBySystem.ContainsKey(sapSystem))
        //    {
        //        _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it's not from {string.Join(", ", _sapConfig.SapServiceConfigsBySystem.Select(x => x.Key))}");
        //        return "Invalid billing system value.";
        //    }
        //    if (string.IsNullOrEmpty(dopplerUser.FederalTaxID))
        //    {
        //        _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a cuit value");
        //        return "Invalid cuit value.";
        //    }
        //    if (!dopplerUser.PlanType.HasValue)
        //    {
        //        _logger.LogInformation($"{dopplerUser.Email} won't be sent to SAP because it doesn't have a plan type id");
        //        return "Invalid plan type value.";
        //    }

        //    return string.Empty;
        //}
    }
}
