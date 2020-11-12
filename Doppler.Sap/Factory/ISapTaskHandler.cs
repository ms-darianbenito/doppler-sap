using System.Threading.Tasks;
using Doppler.Sap.Models;

namespace Doppler.Sap.Factory
{
    public interface ISapTaskHandler
    {
        Task<SapLoginCookies> StartSession();
        Task<SapBusinessPartner> TryGetBusinessPartner(int userId, string cuit, int userPlanTypeId, string countryCode);
        Task<SapBusinessPartner> TryGetBusinessPartnerByCardCode(string cardCode);
        Task<SapTask> CreateBusinessPartnerFromDopplerUser(SapTask task);
    }
}
