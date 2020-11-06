using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Sap.Test
{

    public class IsSuperUserHandlerTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public IsSuperUserHandlerTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        // isSU=true
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.j1qzmKcnpCCBoXAtK9QuzCcnkIedK_kpwlrQ315VX_bwuxNxDBeEgKCOcjACUaNnf92bStGVYxXusSlnCgWApjlFG4TRgcTNsBC_87ZMuTgjP92Ou_IHi5UVDkiIyeQ3S_-XpYGFksgzI6LhSXu2T4LZLlYUHzr6GN68QWvw19m1yw6LdrNklO5qpwARR4WEJVK-0dw2-t4V9jK2kR8zFkTYtDUFPEQaRXFBpaPWAdI1p_Dk_QDkeBbmN_vTNkF7JwmqXRRAaz5fiMmcgzFmayJFbM0Y9LUeaAYFSZytIiYZuNitVixWZEcXT_jwtfHpyDwZKY1-HlyMmUJJuVsf2A")]
        public async Task GetInvoices_WhenToken_ReturnsResponse(HttpStatusCode httpStatusCode, string token)
        {
            // Arrange
            const int clientId = 222541;

            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost/accounts/doppler/{clientId}/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(httpStatusCode, response.StatusCode);
        }
    }
}
