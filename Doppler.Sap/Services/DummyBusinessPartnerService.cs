using Doppler.Sap.Models;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class DummyBusinessPartnerService : IBusinessPartnerService
    {
        public Task CreateOrUpdateBusinessPartner(DopplerUserDto dopplerUser)
        {
            return Task.CompletedTask;
        }
    }
}
