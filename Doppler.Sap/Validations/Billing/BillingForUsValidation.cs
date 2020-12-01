using Doppler.Sap.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Doppler.Sap.Validations.Billing
{
    public class BillingForUsValidation : IBillingValidation
    {
        private const string _sapSystemSupported = "US";
        private readonly ILogger<BillingForUsValidation> _logger;

        public BillingForUsValidation(ILogger<BillingForUsValidation> logger)
        {
            _logger = logger;
        }

        public bool CanCreate(SapBusinessPartner sapBusinessPartner, SapSaleOrderModel billingRequest)
        {
            if (sapBusinessPartner == null)
            {
                _logger.LogError($"Failed at generating billing request for user: {billingRequest.UserId}.");
                return false;
            }

            return true;
        }

        public bool CanValidateSapSystem(string sapSystem)
        {
            return _sapSystemSupported == sapSystem;
        }

        public void ValidateRequest(BillingRequest dopplerBillingRequest)
        {
            if (dopplerBillingRequest.Id.Equals(default))
            {
                _logger.LogError("Billing Request won't be sent to SAP because it doesn't have the user's Id.");
                throw new ArgumentException("Value can not be null", "Id");
            }
        }
    }
}
