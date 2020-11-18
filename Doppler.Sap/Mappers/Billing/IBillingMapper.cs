using Doppler.Sap.Models;

namespace Doppler.Sap.Mappers.Billing
{
    public interface IBillingMapper
    {
        bool CanMapSapSystem(string sapSystem);

        SapSaleOrderModel MapDopplerBillingRequestToSapSaleOrder(BillingRequest billingRequest);
    }
}
