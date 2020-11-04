using Doppler.Sap.Enums;
using Doppler.Sap.Factory;
using Doppler.Sap.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Sap.Test
{
    public class CreateOrUpdateBusinessPartnerHandlerTest
    {
        [Fact]
        public async Task CreateOrUpdateBusinessPartnerHandler_ShouldBeSentNewBPToSap_WhenBPNotExist()
        {
            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    BaseServerUrl = "http://123.123.123/",
                    CompanyDB = "CompanyDb",
                    Password = "password",
                    UserName = "Name"
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

            var sapTask = new SapTask
            {
                DopplerUser = new DopplerUserDto
                {
                    Id = 1,
                    FederalTaxID = "27111111115",
                    PlanType = 1
                },
                TaskType = SapTaskEnum.CreateOrUpdateBusinessPartner
            };

            var sapTaskHandlerMock = new Mock<ISapTaskHandler>();
            sapTaskHandlerMock.Setup(x => x.CreateBusinessPartnerFromDopplerUser(sapTask))
                .ReturnsAsync(new SapTask
                {
                    ExistentBusinessPartner = new SapBusinessPartner { CardCode = "2323423" }
                });
            sapTaskHandlerMock.Setup(x => x.StartSession())
                .ReturnsAsync(new SapLoginCookies
                {
                    B1Session = "session",
                    RouteId = "route"
                });

            var handler = new CreateOrUpdateBusinessPartnerHandler(
                sapConfigMock.Object,
                sapTaskHandlerMock.Object,
                httpClientFactoryMock.Object);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            };
            httpResponseMessage.Headers.Add("Set-Cookie", "");
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var result = await handler.Handle(sapTask);

            Assert.True(result.IsSuccessful);
            Assert.Equal("Creating Business Partner", result.TaskName);
        }

        [Fact]
        public async Task CreateOrUpdateBusinessPartnerHandler_ShouldBeSentUpdateBPToSap_WhenBPExist()
        {
            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    BaseServerUrl = "http://123.123.123/",
                    CompanyDB = "CompanyDb",
                    Password = "password",
                    UserName = "Name"
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

            var sapTask = new SapTask
            {
                DopplerUser = new DopplerUserDto
                {
                    Id = 1,
                    FederalTaxID = "27111111115",
                    PlanType = 1
                },
                TaskType = SapTaskEnum.CreateOrUpdateBusinessPartner
            };

            var sapTaskHandlerMock = new Mock<ISapTaskHandler>();
            sapTaskHandlerMock.Setup(x => x.CreateBusinessPartnerFromDopplerUser(sapTask))
                .ReturnsAsync(new SapTask
                {
                    BusinessPartner = new SapBusinessPartner
                    {
                        BPAddresses = new List<Address>() { },
                        ContactEmployees = new List<SapContactEmployee>() { }
                    },
                    ExistentBusinessPartner = new SapBusinessPartner
                    {
                        CardCode = "2323423",
                        FederalTaxID = "FederalTaxId",
                        ContactEmployees = new List<SapContactEmployee>() { }
                    }
                });
            sapTaskHandlerMock.Setup(x => x.StartSession())
                .ReturnsAsync(new SapLoginCookies
                {
                    B1Session = "session",
                    RouteId = "route"
                });

            var handler = new CreateOrUpdateBusinessPartnerHandler(
                sapConfigMock.Object,
                sapTaskHandlerMock.Object,
                httpClientFactoryMock.Object);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            };
            httpResponseMessage.Headers.Add("Set-Cookie", "");
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var result = await handler.Handle(sapTask);

            Assert.True(result.IsSuccessful);
            Assert.Equal("Updating Business Partner", result.TaskName);
        }
    }
}
