using System.Threading.Tasks;
using Doppler.Sap.Models;

namespace Doppler.Sap.Factory
{
    public interface ISapTaskHandler
    {
        Task<SapLoginCookies> StartSession();
        Task<SapBusinessPartner> TryGetBusinessPartner(SapTask task);
    }
}
