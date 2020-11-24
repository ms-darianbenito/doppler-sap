using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var loggerMock = new Mock<ILogger<BusinessPartnerService>>();
            var queuingServiceMock = new Mock<IQueuingService>();

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    SapServiceConfigsBySystem = new Dictionary<string, SapServiceConfig>
                    {
                        { "AR", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        },
                        { "US", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        }
                    }
                });

            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object, loggerMock.Object, sapConfigMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "27111111115",
                PlanType = 1,
                BillingCountryCode = "US",
                BillingSystemId = 2
            };

            await businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Once);
        }

        [Fact]
        public void BusinessPartnerService_ShouldBeThrowsValidationException_WhenCountryCodeNotAROrUS()
        {
            var loggerMock = new Mock<ILogger<BusinessPartnerService>>();
            var queuingServiceMock = new Mock<IQueuingService>();

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    SapServiceConfigsBySystem = new Dictionary<string, SapServiceConfig>
                    {
                        { "AR", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        }
                    }
                });

            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object, loggerMock.Object, sapConfigMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "27111111115",
                PlanType = 1,
                BillingCountryCode = "MX",
                BillingSystemId = 16
            };

            var ex = Assert.ThrowsAsync<ValidationException>(() => businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser));
            Assert.Equal("Invalid billing system value.", ex.Result.Message);
        }

        [Fact]
        public void BusinessPartnerService_ShouldBeThrowsValidationException_WhenDopplerUserFederalTaxIDIsNotValid()
        {
            var loggerMock = new Mock<ILogger<BusinessPartnerService>>();
            var queuingServiceMock = new Mock<IQueuingService>();

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    SapServiceConfigsBySystem = new Dictionary<string, SapServiceConfig>
                    {
                        { "AR", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        }
                    }
                });

            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object, loggerMock.Object, sapConfigMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "",
                PlanType = 1,
                BillingCountryCode = "AR",
                BillingSystemId = 9
            };

            var ex = Assert.ThrowsAsync<ValidationException>(() => businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser));
            Assert.Equal("Invalid cuit value.", ex.Result.Message);
        }

        [Fact]
        public void BusinessPartnerService_ShouldBeThrowsValidationException_WhenDopplerUserPlanTypeIsNotValid()
        {
            var loggerMock = new Mock<ILogger<BusinessPartnerService>>();
            var queuingServiceMock = new Mock<IQueuingService>();

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    SapServiceConfigsBySystem = new Dictionary<string, SapServiceConfig>
                    {
                        { "AR", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        }
                    }
                });

            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object, loggerMock.Object, sapConfigMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "27111111115",
                BillingCountryCode = "AR",
                BillingSystemId = 9
            };

            var ex = Assert.ThrowsAsync<ValidationException>(() => businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser));
            Assert.Equal("Invalid plan type value.", ex.Result.Message);
        }

        [Fact]
        public void BusinessPartnerService_ShouldBeThrowsValidationException_WhenWhenDopplerUserBillingCountryCodeIsNotValid()
        {
            var loggerMock = new Mock<ILogger<BusinessPartnerService>>();
            var queuingServiceMock = new Mock<IQueuingService>();

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    SapServiceConfigsBySystem = new Dictionary<string, SapServiceConfig>
                    {
                        { "AR", new SapServiceConfig {
                            CompanyDB = "CompanyDb",
                            Password = "password",
                            UserName = "Name",
                            BaseServerUrl = "http://123.123.123/",
                            BusinessPartnerConfig = new BusinessPartnerConfig
                            {
                                Endpoint = "BusinessPartners"
                            }
                        }
                        }
                    }
                });

            var businessPartnerService = new BusinessPartnerService(queuingServiceMock.Object, loggerMock.Object, sapConfigMock.Object);

            var dopplerUser = new DopplerUserDto
            {
                Id = 1,
                FederalTaxID = "27111111115",
                PlanType = 1,
                BillingCountryCode = "MX",
                BillingSystemId = 16
            };

            var ex = Assert.ThrowsAsync<ValidationException>(() => businessPartnerService.CreateOrUpdateBusinessPartner(dopplerUser));
            Assert.Equal("Invalid billing system value.", ex.Result.Message);
        }
    }
}
