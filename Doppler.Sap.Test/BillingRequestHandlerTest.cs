using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Sap.Factory;
using Doppler.Sap.Mappers.Billing;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Doppler.Sap.Utils;
using Doppler.Sap.Validations.Billing;
using Doppler.Sap.Validations.BusinessPartner;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Doppler.Sap.Test
{
    public class BillingRequestHandlerTest
    {
        [Fact]
        public async Task BillingRequestHandler_ShouldBeCreateBillingInSap_WhenQueueHasOneValidElement()
        {
            var billingValidations = new List<IBillingValidation>
            {
                new BillingForArValidation(Mock.Of<ILogger<BillingForArValidation>>()),
                new BillingForUsValidation(Mock.Of<ILogger<BillingForUsValidation>>())
            };

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2019, 09, 25));

            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(Mock.Of<ISapBillingItemsService>()),
                new BillingForUsMapper(Mock.Of<ISapBillingItemsService>(), dateTimeProviderMock.Object)
            };

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
                            },
                            BillingConfig = new BillingConfig
                            {
                                Endpoint = "Orders"
                            }
                        }
                        }
                    }
                });

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"")
                });
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var sapTaskHandlerMock = new Mock<ISapTaskHandler>();
            sapTaskHandlerMock.Setup(x => x.StartSession())
                .ReturnsAsync(new SapLoginCookies
                {
                    B1Session = "session",
                    RouteId = "route"
                });

            sapTaskHandlerMock.Setup(x => x.TryGetBusinessPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new SapBusinessPartner
                {
                    FederalTaxID = "FederalTaxId",
                    CardCode = "2323423"
                });

            var sapServiceSettingsFactoryMock = new Mock<ISapServiceSettingsFactory>();
            sapServiceSettingsFactoryMock.Setup(x => x.CreateHandler("AR")).Returns(sapTaskHandlerMock.Object);

            var handler = new BillingRequestHandler(
                sapConfigMock.Object,
                Mock.Of<ILogger<BillingRequestHandler>>(),
                sapServiceSettingsFactoryMock.Object,
                httpClientFactoryMock.Object,
                billingValidations,
                billingMappers);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            };
            httpResponseMessage.Headers.Add("Set-Cookie", "");
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var result = await handler.Handle(new SapTask
            {
                CurrencyRate = new SapCurrencyRate
                {
                    Currency = "Test"
                },
                BillingRequest = new SapSaleOrderModel
                {
                    BillingSystemId = 9
                }
            });

            Assert.True(result.IsSuccessful);
            Assert.Equal("Creating Billing Request", result.TaskName);
        }

        [Fact]
        public async Task BillingRequestHandler_ShouldBeNotCreateBillingInSap_WhenQueueHasOneElementButBusinessPartnerNotExistsInSAP()
        {
            var billingValidations = new List<IBillingValidation>
            {
                new BillingForArValidation(Mock.Of<ILogger<BillingForArValidation>>()),
                new BillingForUsValidation(Mock.Of<ILogger<BillingForUsValidation>>())
            };

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2019, 09, 25));

            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(Mock.Of<ISapBillingItemsService>()),
                new BillingForUsMapper(Mock.Of<ISapBillingItemsService>(), dateTimeProviderMock.Object)
            };

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"")
                });
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var sapTaskHandlerMock = new Mock<ISapTaskHandler>();
            sapTaskHandlerMock.Setup(x => x.TryGetBusinessPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((SapBusinessPartner)null);

            var sapServiceSettingsFactoryMock = new Mock<ISapServiceSettingsFactory>();
            sapServiceSettingsFactoryMock.Setup(x => x.CreateHandler("AR")).Returns(sapTaskHandlerMock.Object);

            var handler = new BillingRequestHandler(
                sapConfigMock.Object,
                Mock.Of<ILogger<BillingRequestHandler>>(),
                sapServiceSettingsFactoryMock.Object,
                httpClientFactoryMock.Object,
                billingValidations,
                billingMappers);

            var sapTask = new SapTask
            {
                BillingRequest = new SapSaleOrderModel { UserId = 1 }
            };

            var result = await handler.Handle(sapTask);

            Assert.False(result.IsSuccessful);
            Assert.Equal($"Failed at generating billing request for the user: {sapTask.BillingRequest.UserId}.", result.SapResponseContent);
            Assert.Equal("Creating Billing Request", result.TaskName);
        }

        [Fact]
        public async Task BillingRequestHandler_ShouldBeNotCreateBillingInSap_WhenQueueHasOneElementButBusinessPartnerHasFederalTaxIdEmpty()
        {
            var billingValidations = new List<IBillingValidation>
            {
                new BillingForArValidation(Mock.Of<ILogger<BillingForArValidation>>()),
                new BillingForUsValidation(Mock.Of<ILogger<BillingForUsValidation>>())
            };

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2019, 09, 25));

            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(Mock.Of<ISapBillingItemsService>()),
                new BillingForUsMapper(Mock.Of<ISapBillingItemsService>(), dateTimeProviderMock.Object)
            };

            var sapConfigMock = new Mock<IOptions<SapConfig>>();

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"")
                });
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var sapTaskHandlerMock = new Mock<ISapTaskHandler>();
            sapTaskHandlerMock.Setup(x => x.TryGetBusinessPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new SapBusinessPartner
                {
                    FederalTaxID = string.Empty,
                    CardCode = "2323423"
                });

            var sapServiceSettingsFactoryMock = new Mock<ISapServiceSettingsFactory>();
            sapServiceSettingsFactoryMock.Setup(x => x.CreateHandler("AR")).Returns(sapTaskHandlerMock.Object);

            var handler = new BillingRequestHandler(
                sapConfigMock.Object,
                Mock.Of<ILogger<BillingRequestHandler>>(),
                sapServiceSettingsFactoryMock.Object,
                httpClientFactoryMock.Object,
                billingValidations,
                billingMappers);

            var sapTask = new SapTask
            {
                BillingRequest = new SapSaleOrderModel { UserId = 1 }
            };

            var result = await handler.Handle(sapTask);

            Assert.False(result.IsSuccessful);
            Assert.Equal($"Failed at generating billing request for the user: {sapTask.BillingRequest.UserId}.", result.SapResponseContent);
            Assert.Equal("Creating Billing Request", result.TaskName);
        }

        [Fact]
        public async Task BillingRequestHandler_ShouldBeNotCreateBillingInSap_WhenQueueHasOneElementButInvalidCountryCode()
        {
            var sapTask = new SapTask
            {
                BillingRequest = new SapSaleOrderModel { UserId = 1, BillingSystemId = 16 }
            };

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var sapServiceSettingsFactoryMock = new Mock<ISapServiceSettingsFactory>();

            sapServiceSettingsFactoryMock
                .Setup(x => x.CreateHandler(It.IsAny<string>()))
                .Throws(new ArgumentException($"The countryCode '' is not supported."));

            var handler = new BillingRequestHandler(
                sapConfigMock.Object,
                Mock.Of<ILogger<BillingRequestHandler>>(),
                sapServiceSettingsFactoryMock.Object,
                httpClientFactoryMock.Object,
                It.IsAny<IEnumerable<IBillingValidation>>(),
                It.IsAny<IEnumerable<IBillingMapper>>());


            var result = await handler.Handle(sapTask);

            Assert.False(result.IsSuccessful);
            Assert.Equal($"The countryCode '' is not supported.", result.SapResponseContent);
            Assert.Equal("Creating Billing Request", result.TaskName);
        }
    }
}
