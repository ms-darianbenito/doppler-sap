using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Models
{
    public class SapBusinessPartnerList
    {
        [JsonProperty(PropertyName = "odata.metadata")]
        public string metadata { get; set; }
        public List<SapBusinessPartner> value { get; set; }
        [JsonProperty(PropertyName = "odata.nextLink")]
        public string nextLink { get; set; }
    }
}
