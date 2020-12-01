using Doppler.Sap.Models;

namespace Doppler.Sap.Validations.BusinessPartner
{
    public interface IBusinessPartnerValidation
    {
        bool CanValidateSapSystem(string sapSystem);

        bool IsValid(DopplerUserDto dopplerUser, string sapSystem, SapConfig sapConfig, out string error);
    }
}
