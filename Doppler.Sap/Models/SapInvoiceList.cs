using Newtonsoft.Json;
using System.Collections.Generic;

namespace Doppler.Sap.Models
{
    public class SapInvoiceList
    {
        public string Metadata { get; set; }
        public List<SapSaleOrderInvoiceResponse> Value { get; set; }
    }
}
