using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Sap.Test
{
    public class BusinessPartnerServiceTest
    {
        [Fact]
        public async Task BusinessPartnerService_ShouldBeAddTaskInQueue()
        {
            var queuingServiceMock = new Mock<IQueuingService>();
            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "27111111115",
                PlanType = 1
            };

            await businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Once);
        }
    }
}
