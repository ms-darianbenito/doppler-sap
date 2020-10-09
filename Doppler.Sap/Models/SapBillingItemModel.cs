using System.Collections.Generic;

namespace Doppler.Sap.Models
{
    public class SapBillingItemModel
    {
        public int PlanType { get; set; }
        public List<BillingItemPlanDescriptionModel> PlanDescription { get; set; }
    }
}
