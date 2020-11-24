using Doppler.Sap.Enums;

namespace Doppler.Sap.Utils
{
    public class SapSystemHelper
    {
        public static string GetSapSystemByBillingSystem(int billingSystem)
        {
            switch ((ResponsabileBillingEnum)billingSystem)
            {
                case ResponsabileBillingEnum.GBBISIDE:
                case ResponsabileBillingEnum.Mercadopago:
                    return "AR";
                case ResponsabileBillingEnum.QuickBookUSA:
                case ResponsabileBillingEnum.QBL:
                    return "US";
                default:
                    return "";
            }
        }
    }
}
