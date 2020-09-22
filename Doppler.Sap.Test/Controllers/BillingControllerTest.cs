using Doppler.Sap.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using Xunit;

namespace Doppler.Sap.Test.Controllers
{
    public class BillingControllerTest
    {
        [Fact]
        public void SetCurrencyRate_ShouldBeHttpStatusCodeOk_WhenCurrencyRateValid()
        {
            var loggerMock = new Mock<ILogger<BillingController>>();
            var controller = new BillingController(loggerMock.Object);

            // Act
            var response = controller.SetCurrencyRate();

            // Assert
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)((ObjectResult)response).StatusCode.Value);
            Assert.Equal("Works fine!!", ((ObjectResult)response).Value);
        }
    }
}
