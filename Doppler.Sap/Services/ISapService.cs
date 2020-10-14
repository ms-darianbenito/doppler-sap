using System.Threading.Tasks;
using Doppler.Sap.Models;

namespace Doppler.Sap.Services
{
    public interface ISapService
    {
        public Task<SapTaskResult> SendToSap(SapTask dequeuedTask);

        public Task<SapBusinessPartner> TryGetBusinessPartner(string cardCode, string cuit);
        public Task<SapBusinessPartner> TryGetBusinessPartnerByCardCode(string cardCode);
    }
}
