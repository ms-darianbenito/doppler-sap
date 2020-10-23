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
            if (string.IsNullOrEmpty(dopplerUser.FederalTaxID))
            {
                _queuingService.AddToTaskQueue(
                    new SapTask()
                    {
                        TaskType = SapTaskEnum.CreateBusinessPartner,
                        DopplerUser = dopplerUser
                    }
                );
            }
            else
            {
                _queuingService.AddToTaskQueue(
                    new SapTask()
                    {
                        TaskType = SapTaskEnum.UpdateBusinessPartner,
                        DopplerUser = dopplerUser
                    }
                );
            }

            return Task.CompletedTask;
        }
    }
}
