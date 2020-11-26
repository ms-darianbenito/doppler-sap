using Doppler.Sap.Models;
using System.Collections.Generic;

namespace Doppler.Sap.Mappers.BusinessPartner
{
    public abstract class BusinessPartnerMapper
    {
        /// <summary>
        /// Key: PlanType; Value: GroupCode
        /// </summary>
        protected abstract Dictionary<int, int> DopplerGroupCodes { get; }

        /// <summary>
        /// Key: ClientManagerType; Value: GroupCode
        /// </summary>
        protected abstract Dictionary<int, int> ClientManagerGroupCodes { get; }

        /// <summary>
        /// Key: PlanType; Value: GroupCode
        /// </summary>
        protected abstract Dictionary<int, int> RelayGroupCodes { get; }

        protected int MapGroupCode(DopplerUserDto dopplerUser)
        {
            var groupCode = (!dopplerUser.IsClientManager && !dopplerUser.IsFromRelay) ?
                            (dopplerUser.PlanType.HasValue ? (DopplerGroupCodes.TryGetValue(dopplerUser.PlanType.Value, out var dopplerGroupCode) ? dopplerGroupCode : 0) : 0) :
                            (dopplerUser.IsClientManager) ?
                            ClientManagerGroupCodes.TryGetValue(dopplerUser.ClientManagerType, out var clientManagerGroupCode) ? clientManagerGroupCode : 0 :
                            (dopplerUser.PlanType.HasValue ? (RelayGroupCodes.TryGetValue(dopplerUser.PlanType.Value, out var relayGroupCode) ? relayGroupCode : 0) : 0);

            return groupCode;
        }
    }
}
