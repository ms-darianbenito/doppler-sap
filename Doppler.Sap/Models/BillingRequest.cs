namespace Doppler.Sap.Models
{
    public class BillingRequest
    {
        public int InvoiceId { get; set; }
        public int Id { get; set; }
        public int PlanType { get; set; }
        public int CreditsOrSubscribersQuantity { get; set; }
        public bool IsCustomPlan { get; set; }
        public bool IsPlanUpgrade { get; set; }
        public int Currency { get; set; }
        public int? Periodicity { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public double PlanFee { get; set; }
        public int? Discount { get; set; }
        public int? ExtraEmails { get; set; }
        public double? ExtraEmailsFeePerUnit { get; set; }
        public int ExtraEmailsPeriodMonth { get; set; }
        public int ExtraEmailsPeriodYear { get; set; }
        public double ExtraEmailsFee { get; set; }
        public string PurchaseOrder { get; set; }
        public string FiscalID { get; set; }
        public int BillingSystemId { get; set; } = 9;
        public string CardHolder { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CardErrorCode { get; set; } = "100";
        public string CardErrorDetail { get; set; }
        public bool TransactionApproved { get; set; }
        public string TransferReference { get; set; }
    }
}
