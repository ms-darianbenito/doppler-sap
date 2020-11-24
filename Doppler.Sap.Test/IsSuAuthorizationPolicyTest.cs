using Billing.API.Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.Sap.Test
{
    public class IsSuAuthorizationPolicyTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public IsSuAuthorizationPolicyTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        // isSU: true
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.j1qzmKcnpCCBoXAtK9QuzCcnkIedK_kpwlrQ315VX_bwuxNxDBeEgKCOcjACUaNnf92bStGVYxXusSlnCgWApjlFG4TRgcTNsBC_87ZMuTgjP92Ou_IHi5UVDkiIyeQ3S_-XpYGFksgzI6LhSXu2T4LZLlYUHzr6GN68QWvw19m1yw6LdrNklO5qpwARR4WEJVK-0dw2-t4V9jK2kR8zFkTYtDUFPEQaRXFBpaPWAdI1p_Dk_QDkeBbmN_vTNkF7JwmqXRRAaz5fiMmcgzFmayJFbM0Y9LUeaAYFSZytIiYZuNitVixWZEcXT_jwtfHpyDwZKY1-HlyMmUJJuVsf2A")]
        // "isSU": false
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ3MDksImV4cCI6MTU5Nzc2NDcxOSwiaWF0IjoxNTk3NzY0NzA5LCJpc1NVIjpmYWxzZX0.K7khi_qhvj0eF3ahZzNcRkzrRPDFR_q-5xAujSeFG3GaFhJIhgARX7fsA4iPPhTJtFA1oqF54d-vyNhGAhBDFzSKUHyRegdRJ5FiQwcQ537PbZUfCc702sEi-MjzfpkP1PZrk0Zrn5-ybUDJi-6qjia8_YxvA4px8KGPT10Z6PnrpeCuWtESmMlSre7CgCRpydXZ0XkV0hsn-CD8p5oSV9iMCXS3npJBBhzLvw9B_LienlnJQMVs88ykSDqZNUWdGMVTO4QF4JChd67W7B9I0MmmbtgCZ5yo0EwykYR6RaZYihtKjesmHlBcFaHJc1C-3V8TQ3L0-81PpemqZd_3yQ")]
        // without isSU
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE2MDUwMjIxNDMsImV4cCI6MTYwNTAyMjIwMSwiaWF0IjoxNjA1MDIyMTQzfQ.j9rYDxZZ2EQ_twu8-KrkF9TZbKwh6BgLYkc8gtjFRt0jFl3in5DzDN_0qxsbcMcHyLtFus8HfnmXjFj9bZbUD2AOijPtAD3McMoQfBQS5YF4Kp-coAnweKTp_3mEW4slPqM_OBMbkRP4n1xux90p3YeJ-pYwWC3v7t1ddGWO2BHInRVb1ztcpyYWal9h3TfNyUexrRHhxW74qP59CXjl7vqsBFBeLEUky79-dGIHRhECSG4zaHiMYvFY9X7em2ERCENF0mQrCZYPxjDXWDlEuPH2tKRXEGFXHGwoaM7ZnH07_bx3ngUhjtvsjjUtDDQ6lwJMvbQODj1ANtAl-YL8ow")]
        public async Task CreateOrUpdateBusinessPartner_WhenToken_ReturnsResponse(HttpStatusCode httpStatusCode, string token)
        {
            using var appFactory = _factory.WithDisabledLifeTimeValidation();
            var client = appFactory.CreateClient();
            var businessPartner = new
            {
                BillingCountryCode = "AR",
                FederalTaxID = "27111111115",
                PlanType = 1,
                BillingSystemId = 9
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(businessPartner), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5001/BusinessPartner/CreateOrUpdateBusinessPartner");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = requestContent;

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(httpStatusCode, response.StatusCode);
        }
    }
}
