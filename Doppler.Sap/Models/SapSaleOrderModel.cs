using System;
using System.Collections.Generic;

namespace Doppler.Sap.Models
{
    public class SapSaleOrderModel
    {
        public string CardCode { get; set; }
        public string DocDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public string DocDueDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public string TaxDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public string NumAtCard { get; set; }
        public string U_DPL_RECURRING_SERV { get; set; }
        public List<SapDocumentLineModel> DocumentLines { get; set; }
        public string FiscalID { get; set; }
        public int UserId { get; set; }
        public int PlanType { get; set; }
        public int BillingSystemId { get; set; } = 9;
        public string U_DPL_CARD_HOLDER { get; set; }
        public string U_DPL_CARD_NUMBER { get; set; }
        public string U_DPL_CARD_TYPE { get; set; }
        public string U_DPL_CARD_ERROR_COD { get; set; }
        public string U_DPL_CARD_ERROR_DET { get; set; }
    }
}
