using System;

namespace Doppler.Sap.Models
{
    public class SapLoginCookies
    {
        public string B1Session { get; set; }
        public string RouteId { get; set; }
        public DateTime SessionEndAt { get; set; }
    }
}
