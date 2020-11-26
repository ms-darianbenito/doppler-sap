using Doppler.Sap.Mappers.BusinessPartner;
using Doppler.Sap.Models;
using Xunit;

namespace Doppler.Sap.Test.Mappers
{
    public class BusinessPartnerForUsMapperTest
    {
        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodeAltoVolumen_WhenIsDopplerAndPlanTypeEqual2()
        {
            var groupCode = 103;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 2,
                IsClientManager = false,
                IsFromRelay = false,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }

        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodePrepago_WhenIsDopplerAndPlanTypeEqual3()
        {
            var groupCode = 102;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 3,
                IsClientManager = false,
                IsFromRelay = false,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }

        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodeContactos_WhenIsDopplerAndPlanTypeEqual4()
        {
            var groupCode = 100;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 4,
                IsClientManager = false,
                IsFromRelay = false,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }

        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodeClientManager_WhenIsClientManagerAndClientManagerTypeEqual1()
        {
            var groupCode = 104;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 0,
                IsClientManager = true,
                ClientManagerType = 1,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }

        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodeClientManagerResel_WhenIsClientManagerAndClientManagerTypeEqual2()
        {
            var groupCode = 105;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 0,
                IsClientManager = true,
                ClientManagerType = 2,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }

        [Fact]
        public void BusinessPartnerForUsMapper_ShouldBeSetGroupCodeRelay_WhenIsFromRelay()
        {
            var groupCode = 106;
            var dopplerUserDto = new DopplerUserDto
            {
                PlanType = 5,
                IsFromRelay = true,
                PaymentMethod = 1,
                FirstName = "Juan",
                LastName = "Perez",
                FederalTaxID = "123",
                BillingStateId = "01",
                Email = "test@test.com",
                BillingSystemId = 2
            };

            var mapper = new BusinessPartnerForUsMapper();
            var sapBusinessPartner = mapper.MapDopplerUserToSapBusinessPartner(dopplerUserDto, "CD00001", null);

            Assert.Equal(groupCode, sapBusinessPartner.GroupCode);
        }
    }
}
