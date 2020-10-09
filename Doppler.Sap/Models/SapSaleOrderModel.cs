using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
