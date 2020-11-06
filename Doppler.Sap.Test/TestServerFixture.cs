using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Doppler.Sap.Test
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; }

        public HttpClient Client { get; }

        public TestServerFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .ConfigureTestServices(services => { });

            Server = new TestServer(builder);
            Client = Server.CreateClient();

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            Server.Dispose();
            Client.Dispose();
        }
    }
}
