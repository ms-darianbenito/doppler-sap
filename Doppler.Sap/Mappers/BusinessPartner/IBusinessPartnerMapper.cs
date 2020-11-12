using Doppler.Sap.Models;
using System.Collections.Specialized;

namespace Doppler.Sap.Mappers.BusinessPartner
{
    public interface IBusinessPartnerMapper
    {
        bool CanMapCountry(string countryCode);

        string MapDopplerUserIdToSapBusinessPartnerId(int id, int planType);
        SapBusinessPartner MapDopplerUserToSapBusinessPartner(DopplerUserDto dopplerUser, string cardCode, SapBusinessPartner fatherBusinessPartner);
    }
}
