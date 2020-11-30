using Doppler.Sap.Models;
using Doppler.Sap.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Doppler.Sap.Mappers.Billing
{
    public class BillingForArMapper : IBillingMapper
    {
        private const string _sapSystemSupported = "AR";
        private const string _costingCode1 = "1000";
        private const string _costingCode2 = "1100";
        private const string _costingCode3 = "Arg";
        private const string _costingCode4 = "NOAPLI4";

        private readonly ISapBillingItemsService _sapBillingItemsService;

        public BillingForArMapper(ISapBillingItemsService sapBillingItemsService)
        {
            _sapBillingItemsService = sapBillingItemsService;
        }

        public bool CanMapSapSystem(string sapSystem)
        {
            return _sapSystemSupported == sapSystem;
        }

        public SapSaleOrderModel MapDopplerBillingRequestToSapSaleOrder(BillingRequest billingRequest)
        {
            var sapSaleOrder = new SapSaleOrderModel
            {
                NumAtCard = billingRequest.PurchaseOrder ?? "",
                U_DPL_RECURRING_SERV = billingRequest.IsPlanUpgrade ? "N" : "Y",
                DocumentLines = new List<SapDocumentLineModel>()
            };
            var currencyCode = Dictionary.CurrencyDictionary.TryGetValue(billingRequest.Currency, out var code) ? code : "";

            var itemCode = _sapBillingItemsService.GetItemCode(billingRequest.PlanType, billingRequest.CreditsOrSubscribersQuantity, billingRequest.IsCustomPlan);

            var planItem = new SapDocumentLineModel
            {
                ItemCode = itemCode,
                UnitPrice = billingRequest.PlanFee,
                Currency = currencyCode,
                FreeText = $"{currencyCode} {billingRequest.PlanFee.ToString(CultureInfo.CurrentCulture)} + IMP",
                DiscountPercent = billingRequest.Discount ?? 0,
                CostingCode = _costingCode1,
                CostingCode2 = _costingCode2,
                CostingCode3 = _costingCode3,
                CostingCode4 = _costingCode4
            };

            var freeText = new
            {
                Amount = $"{currencyCode} {billingRequest.PlanFee.ToString(CultureInfo.CurrentCulture)} + IMP",
                Periodicity = billingRequest.Periodicity != null ? $"Plan {(Dictionary.PeriodicityDictionary.TryGetValue(billingRequest.Periodicity, out var outPeriodicity) ? outPeriodicity : string.Empty)}" : null,
                Discount = billingRequest.Discount > 0 ? $"Descuento {billingRequest.Discount}%" : null,
                Payment = $"Abono {billingRequest.PeriodMonth:00} {billingRequest.PeriodYear}",
            };

            planItem.FreeText = string.Join(" - ", new string[] { freeText.Amount, freeText.Periodicity, freeText.Discount, freeText.Payment }.Where(s => !string.IsNullOrEmpty(s)));

            sapSaleOrder.DocumentLines.Add(planItem);

            if (billingRequest.ExtraEmails > 0)
            {
                var itemCodeSurplus = _sapBillingItemsService.GetItems(billingRequest.PlanType).Where(x => x.SurplusEmails.Value)
                    .Select(x => x.ItemCode)
                    .FirstOrDefault();

                var extraEmailItem = new SapDocumentLineModel
                {
                    ItemCode = itemCodeSurplus,
                    UnitPrice = billingRequest.ExtraEmailsFee,
                    Currency = currencyCode,
                    FreeText = $"Email excedentes {billingRequest.ExtraEmails}",
                    CostingCode = _costingCode1,
                    CostingCode2 = _costingCode2,
                    CostingCode3 = _costingCode3,
                    CostingCode4 = _costingCode4
                };

                if (billingRequest.ExtraEmailsFee > 0)
                {
                    extraEmailItem.FreeText +=
                        $" - {currencyCode} {billingRequest.ExtraEmailsFeePerUnit} + IMP";
                }

                extraEmailItem.FreeText +=
                    $" - Período {billingRequest.ExtraEmailsPeriodMonth:00} {billingRequest.ExtraEmailsPeriodYear}";

                sapSaleOrder.DocumentLines.Add(extraEmailItem);
            }

            sapSaleOrder.FiscalID = billingRequest.FiscalID;
            sapSaleOrder.UserId = billingRequest.Id;
            sapSaleOrder.PlanType = billingRequest.PlanType;

            return sapSaleOrder;
        }
    }
}
