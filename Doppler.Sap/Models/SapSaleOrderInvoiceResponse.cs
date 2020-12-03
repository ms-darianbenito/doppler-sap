using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Models
{
    public class SapSaleOrderInvoiceResponse
    {
        public string CardCode { get; set; }
        public int DocEntry { get; set; }
        public DateTime DocDate { get; set; }
        public decimal DocTotal { get; set; }

    }
}
