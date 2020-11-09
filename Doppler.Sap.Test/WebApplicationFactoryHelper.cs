using Doppler.Sap;
using Doppler.Sap.DopplerSecurity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Billing.API.Test
{
    public static class WebApplicationFactoryHelper
    {
        public static WebApplicationFactory<Startup> ConfigureService<TOptions>(this WebApplicationFactory<Startup> factory, Action<TOptions> configureOptions) where TOptions : class
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(
                    services => services.Configure(configureOptions)));

        public static WebApplicationFactory<Startup> ConfigureSecurityOptions(this WebApplicationFactory<Startup> factory, Action<DopplerSecurityOptions> configureOptions)
            => factory.ConfigureService(configureOptions);

        public static WebApplicationFactory<Startup> WithDisabledLifeTimeValidation(this WebApplicationFactory<Startup> factory)
            => factory.ConfigureSecurityOptions(
                o => o.SkipLifetimeValidation = true);
    }
}
