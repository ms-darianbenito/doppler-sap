using Doppler.Sap.Enums;
using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly IQueuingService _queuingService;
        private readonly ISapService _sapService;

        public BusinessPartnerService(
            IQueuingService queuingService,
            ISapService sapService)
        {
            _queuingService = queuingService;
            _sapService = sapService;
        }

        public async Task CreateOrUpdateBusinessPartner(DopplerUserDto dopplerUser)
        {
            var existentBusinessPartner = await GetBusinessPartnerIfExists(dopplerUser.Id, dopplerUser.FederalTaxID, dopplerUser.planType.Value);

            var fatherCard = dopplerUser.GroupCode == 115 ?
                    $"CR{dopplerUser.Id:0000000000000}" :
                    (dopplerUser.IsClientManager ?
                    $"CD{int.Parse("400" + dopplerUser.Id.ToString()):0000000000000}" :
                    $"CD{dopplerUser.Id:0000000000000}");

            var fatherBusinessPartner = await _sapService.TryGetBusinessPartnerByCardCode(fatherCard);
            if (fatherBusinessPartner == null && !existentBusinessPartner.CardCode.EndsWith(".0"))
            {
                fatherCard = existentBusinessPartner.CardCode.Replace(existentBusinessPartner.CardCode.Substring(existentBusinessPartner.CardCode.IndexOf(".")), ".0");
                fatherBusinessPartner = await _sapService.TryGetBusinessPartnerByCardCode(fatherCard);
            }

            var newBusinessPartner = BusinessPartnerMapper.MapDopplerUserToSapBusinessPartner(dopplerUser, existentBusinessPartner.CardCode, fatherBusinessPartner);

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
            var cardcode = BusinessPartnerMapper.MapDopplerUserIdToSapBusinessPartnerId(userId, userPlanTypeId);

            return await _sapService.TryGetBusinessPartner(cardcode, cuit);
        }
    }
}
