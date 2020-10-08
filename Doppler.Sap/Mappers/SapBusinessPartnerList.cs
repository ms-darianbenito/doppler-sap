using System.Collections.Generic;
using Doppler.Sap.Models;
using Newtonsoft.Json;

namespace Doppler.Sap.Mappers
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
