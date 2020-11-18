using Doppler.Sap.Models;

namespace Doppler.Sap.Validations.Billing
{
    public interface IBillingValidation
    {
        bool CanCreate(SapBusinessPartner sapBusinessPartner, SapSaleOrderModel billingRequest);

        bool CanValidateSapSystem(string sapSystem);

        bool IsValidRequest(BillingRequest dopplerBillingRequest);
    }
}
