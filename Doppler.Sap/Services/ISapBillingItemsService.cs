using Doppler.Sap.Models;
using System.Collections.Generic;

namespace Doppler.Sap.Services
{
    public interface ISapBillingItemsService
    {
        string GetItemCode(int planType, int creditsOrSubscribersQuantity, bool isCustomPlan);

        List<BillingItemPlanDescriptionModel> GetItems(int planType);
    }
}
