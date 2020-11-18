using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Sap.Factory;
using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Doppler.Sap.Test
{
    public class SapTaskHandlerTest
    {
        [Fact]
        public async Task SapTaskHandler_ShouldBeUseSameCookies_WhenTimeSessionIsMinorThat30Minutes()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{'SessionTimeout': 30}")
            };
            httpResponseMessage.Headers.Add("Set-Cookie", new[] { "B1SESSION=3e560b10-0e46-11e3-8004-1c96ec300ae4;HttpOnly;", "ROUTEID=.test1;path=/b1s" });
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2019, 09, 25));

            var sapTaskHandler = new SapTaskHandler(
                new SapConfig { },
                Mock.Of<ILogger<SapTaskHandler>>(),
                httpClientFactoryMock.Object,
                dateTimeProviderMock.Object,
                new SapServiceConfig { CompanyDB = "CompanyDb", Password = "password", UserName = "Name", BaseServerUrl = "http://123.123.123" },
                null);

            var cookiesFirst = await sapTaskHandler.StartSession();
            var cookiesSecond = await sapTaskHandler.StartSession();

            httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            Assert.Equal(cookiesSecond, cookiesFirst);
        }

        [Fact]
        public async Task SapTaskHandler_ShouldBeUseDifferentCookies_WhenTimeSessionIsExpired()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{'SessionTimeout': 30}")
            };
            httpResponseMessage.Headers.Add("Set-Cookie", new[] { "B1SESSION=3e560b10-0e46-11eb-8000-1c98ec3e0ag4;HttpOnly;", "ROUTEID=.test1;path=/b1as" });
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var date = new DateTime(2019, 09, 25);
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(date);

            var sapConfigMock = new Mock<IOptions<SapConfig>>();
            sapConfigMock.Setup(x => x.Value)
                .Returns(new SapConfig
                {
                    //BaseServerUrl = "http://123.123.123"
                });

            var sapTaskHandler = new SapTaskHandler(
                new SapConfig(),
                Mock.Of<ILogger<SapTaskHandler>>(),
                httpClientFactoryMock.Object,
                dateTimeProviderMock.Object,
                new SapServiceConfig { CompanyDB = "CompanyDb", Password = "password", UserName = "Name", BaseServerUrl = "http://123.123.123" },
                null);

            var cookiesFirst = await sapTaskHandler.StartSession();

            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(date.AddDays(1));

            var cookiesSecond = await sapTaskHandler.StartSession();

            httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Exactly(2));
            Assert.NotEqual(cookiesSecond, cookiesFirst);
        }
    }
}
