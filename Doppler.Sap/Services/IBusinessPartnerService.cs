using Doppler.Sap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public interface IBusinessPartnerService
    {
        Task CreateOrUpdateBusinessPartner(DopplerUserDTO dopplerUser);
    }
}
