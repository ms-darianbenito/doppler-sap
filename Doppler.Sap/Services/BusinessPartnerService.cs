using Doppler.Sap.Enums;
using Doppler.Sap.Factory;
using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly IQueuingService _queuingService;

        public BusinessPartnerService(
            IQueuingService queuingService)
        {
            _queuingService = queuingService;
        }

        public Task CreateOrUpdateBusinessPartner(DopplerUserDto dopplerUser)
        {
            _queuingService.AddToTaskQueue(
                new SapTask()
                {
                    TaskType = SapTaskEnum.CreateOrUpdateBusinessPartner,
                    DopplerUser = dopplerUser
                }
            );
            return Task.CompletedTask;
        }
    }
}
