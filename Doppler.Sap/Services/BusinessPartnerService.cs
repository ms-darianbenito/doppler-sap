using Doppler.Sap.Enums;
using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly BusinessPartnerMapper _businessPartnerMapper;
        private readonly IQueuingService _queuingService;
        private readonly ISapService _sapService;


        public BusinessPartnerService(
            BusinessPartnerMapper businessPartnerMapper,
            IQueuingService queuingService,
            ISapService sapService)
        {
            _businessPartnerMapper = businessPartnerMapper;
            _queuingService = queuingService;
            _sapService = sapService;
        }

        public async Task CreateOrUpdateBusinessPartner(DopplerUserDTO dopplerUser)
        {
            var existentBusinessPartner = await GetBusinessPartnerIfExists(dopplerUser.Id, dopplerUser.FederalTaxID, dopplerUser.planType.Value);
            var newBusinessPartner = await _businessPartnerMapper.MapDopplerUserToSAPBusinessPartner(dopplerUser, existentBusinessPartner.CardCode);

            if (string.IsNullOrEmpty(existentBusinessPartner.FederalTaxID))
            {
                _queuingService.AddToTaskQueue(
                    new SapTask()
                    {
                        TaskType = SapTaskEnum.CreateBusinessPartner,
                        BusinessPartner = newBusinessPartner
                    }
                );
            }
            else
            {
                _queuingService.AddToTaskQueue(
                    new SapTask()
                    {
                        TaskType = SapTaskEnum.UpdateBusinessPartner,
                        BusinessPartner = newBusinessPartner,
                        ExistentBusinessPartner = existentBusinessPartner
                    }
                );
            }
        }

        private async Task<SapBusinessPartner> GetBusinessPartnerIfExists(int userId, string cuit, int userPlanTypeId)
        {
            var cardcode = _businessPartnerMapper.MapDopplerUserIdToSapBusinessPartnerId(userId, userPlanTypeId);

            return await _sapService.TryGetBusinessPartner(cardcode, cuit);
        }
    }
}
