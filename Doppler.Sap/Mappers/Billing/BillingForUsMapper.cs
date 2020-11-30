using Doppler.Sap.Models;
using Doppler.Sap.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Doppler.Sap.Mappers.Billing
{
    public class BillingForUsMapper : IBillingMapper
    {
        private const string _sapSystemSupported = "US";
        private const string _defaultTaxCode = "Exempt";
        private const string _costingCode1 = "1000";
        private const string _costingCode2 = "1100";
        private const string _costingCode3 = "USA";
        private const string _costingCode4 = "NOAPL4";
        private const string _currencyCode = "$";

        private readonly ISapBillingItemsService _sapBillingItemsService;

        private readonly Dictionary<int?, string> periodicities = new Dictionary<int?, string>
        {
            {0, "Monthly"},
            {1, "Quarterly"},
            {2, "Biannual"},
            {3, "Annual"}
        };


        public BillingForUsMapper(ISapBillingItemsService sapBillingItemsService)
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
                U_DPL_CARD_HOLDER = billingRequest.CardHolder,
                U_DPL_CARD_NUMBER = billingRequest.CardNumber,
                U_DPL_CARD_TYPE = billingRequest.CardType,
                U_DPL_CARD_ERROR_COD = billingRequest.CardErrorCode,
                U_DPL_CARD_ERROR_DET = billingRequest.CardErrorDetail,
                DocumentLines = new List<SapDocumentLineModel>()
            };

            var itemCode = _sapBillingItemsService.GetItemCode(billingRequest.PlanType, billingRequest.CreditsOrSubscribersQuantity, billingRequest.IsCustomPlan);

            var planItem = new SapDocumentLineModel
            {
                TaxCode = _defaultTaxCode,
                ItemCode = itemCode,
                UnitPrice = billingRequest.PlanFee,
                Currency = _currencyCode,
                FreeText = $"{_currencyCode} {billingRequest.PlanFee.ToString(CultureInfo.CurrentCulture)} + TAX",
                DiscountPercent = billingRequest.Discount ?? 0,
                CostingCode = _costingCode1,
                CostingCode2 = _costingCode2,
                CostingCode3 = _costingCode3,
                CostingCode4 = _costingCode4
            };

            var freeText = new
            {
                Amount = $"{_currencyCode} {billingRequest.PlanFee.ToString(CultureInfo.CurrentCulture)} + TAX",
                Periodicity = billingRequest.Periodicity != null ? $" {(periodicities.TryGetValue(billingRequest.Periodicity, out var outPeriodicity2) ? outPeriodicity2 : string.Empty)} Plan " : null,
                Discount = billingRequest.Discount > 0 ? $"{billingRequest.Discount}% OFF" : null,
                Payment = $"Payment {billingRequest.PeriodMonth:00} {billingRequest.PeriodYear}",
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
                    TaxCode = _defaultTaxCode,
                    ItemCode = itemCodeSurplus,
                    UnitPrice = billingRequest.ExtraEmailsFee,
                    Currency = _currencyCode,
                    FreeText = $"Excess emails {billingRequest.ExtraEmails}",
                    CostingCode = _costingCode1,
                    CostingCode2 = _costingCode2,
                    CostingCode3 = _costingCode3,
                    CostingCode4 = _costingCode4
                };

                var extraEmailsFreeText = new
                {
                    ExcessEmails = $"Excess emails {billingRequest.ExtraEmails}",
                    Amount = billingRequest.ExtraEmailsFee > 0 ? $"{_currencyCode} {billingRequest.ExtraEmailsFeePerUnit} + TAX" : null
                };


                if (billingRequest.ExtraEmailsFee > 0)
                {
                    extraEmailItem.FreeText += $" - {_currencyCode} {billingRequest.ExtraEmailsFeePerUnit} + TAX";
                }

                extraEmailItem.FreeText += $" - Period {billingRequest.ExtraEmailsPeriodMonth:00} {billingRequest.ExtraEmailsPeriodYear}";

                var test = string.Join(" - ", new string[] { extraEmailsFreeText.ExcessEmails, extraEmailsFreeText.Amount }.Where(s => !string.IsNullOrEmpty(s)));

                sapSaleOrder.DocumentLines.Add(extraEmailItem);
            }

            sapSaleOrder.FiscalID = billingRequest.FiscalID;
            sapSaleOrder.UserId = billingRequest.Id;
            sapSaleOrder.PlanType = billingRequest.PlanType;
            sapSaleOrder.BillingSystemId = billingRequest.BillingSystemId;

            return sapSaleOrder;
        }
    }
}
