using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.Sap.Mappers.Billing;
using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Doppler.Sap.Utils;
using Doppler.Sap.Validations.Billing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Doppler.Sap.Test
{
    public class BillingServiceTest
    {
        [Fact]
        public async Task BillingService_ShouldNotBeAddTaskInQueue_WhenCurrencyRateListIsEmpty()
        {
            var queuingServiceMock = new Mock<IQueuingService>();
            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(null),
                new BillingForUsMapper(null)
            };

            var billingService = new BillingService(queuingServiceMock.Object,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<ILogger<BillingService>>(),
                Mock.Of<ISlackService>(),
                billingMappers,
                null);

            var currencyList = new List<CurrencyRateDto>();

            await billingService.SendCurrencyToSap(currencyList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Never);
        }

        [Fact]
        public async Task BillingService_ShouldBeAddTaskInQueue_WhenCurrencyRateListHasOneValidElement()
        {
            var queuingServiceMock = new Mock<IQueuingService>();
            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(null),
                new BillingForUsMapper(null)
            };

            var billingService = new BillingService(queuingServiceMock.Object,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<ILogger<BillingService>>(),
                Mock.Of<ISlackService>(),
                billingMappers,
                null);

            var currencyList = new List<CurrencyRateDto>
            {
                new CurrencyRateDto
                {
                    SaleValue = 3,
                    CurrencyCode = "ARS",
                    CurrencyName = "Pesos Argentinos",
                    Date = DateTime.Now
                }
            };

            await billingService.SendCurrencyToSap(currencyList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Once);
        }

        [Fact]
        public async Task BillingService_ShouldBeAddThreeTasksInQueue_WhenListHasOneValidElementWithFridayDay()
        {
            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(null),
                new BillingForUsMapper(null)
            };

            var queuingServiceMock = new Mock<IQueuingService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2020, 09, 25));

            var billingService = new BillingService(queuingServiceMock.Object,
                dateTimeProviderMock.Object,
                Mock.Of<ILogger<BillingService>>(),
                Mock.Of<ISlackService>(),
                billingMappers,
                null);

            var currencyList = new List<CurrencyRateDto>
            {
                new CurrencyRateDto
                {
                    SaleValue = 3,
                    CurrencyCode = "ARS",
                    CurrencyName = "Pesos Argentinos",
                    Date = DateTime.UtcNow
                }
            };

            await billingService.SendCurrencyToSap(currencyList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Exactly(3));
        }


        [Fact]
        public async Task BillingService_ShouldBeAddOneTaskInQueue_WhenBillingRequestListHasOneValidElement()
        {
            var itemCode = "1.0.1";
            var items = new List<BillingItemPlanDescriptionModel>
            {
                new BillingItemPlanDescriptionModel
                {
                    ItemCode = "1.0.1",
                    description = "Test"
                }
            };

            var billingValidations = new List<IBillingValidation>
            {
                new BillingForArValidation(Mock.Of<ILogger<BillingForArValidation>>()),
                new BillingForUsValidation(Mock.Of<ILogger<BillingForUsValidation>>())
            };

            var sapBillingItemsServiceMock = new Mock<ISapBillingItemsService>();
            sapBillingItemsServiceMock.Setup(x => x.GetItemCode(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(itemCode);
            sapBillingItemsServiceMock.Setup(x => x.GetItems(It.IsAny<int>())).Returns(items);

            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(sapBillingItemsServiceMock.Object),
                new BillingForUsMapper(sapBillingItemsServiceMock.Object)
            };

            var queuingServiceMock = new Mock<IQueuingService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow)
                .Returns(new DateTime(2020, 09, 25));

            var billingService = new BillingService(queuingServiceMock.Object,
                dateTimeProviderMock.Object,
                Mock.Of<ILogger<BillingService>>(),
                Mock.Of<ISlackService>(),
                billingMappers,
                billingValidations);

            var billingRequestList = new List<BillingRequest>
            {
                new BillingRequest
                {
                    BillingSystemId = 9,
                    Id =1,
                    FiscalID = "123"
                }
            };

            await billingService.CreateBillingRequest(billingRequestList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Once);
        }

        [Fact]
        public async Task BillingService_ShouldBeAddOneTaskInQueue_WhenBillingRequestListHasOneInvalidElement()
        {
            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(Mock.Of<ISapBillingItemsService>()),
                new BillingForUsMapper(Mock.Of<ISapBillingItemsService>())
            };

            var slackServiceMock = new Mock<ISlackService>();
            slackServiceMock.Setup(x => x.SendNotification(It.IsAny<string>())).Returns(Task.CompletedTask);

            var queuingServiceMock = new Mock<IQueuingService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            var billingService = new BillingService(queuingServiceMock.Object,
                dateTimeProviderMock.Object,
                Mock.Of<ILogger<BillingService>>(),
                slackServiceMock.Object,
                billingMappers,
                null);

            var billingRequestList = new List<BillingRequest>
            {
                new BillingRequest
                {
                    BillingSystemId = 9,
                    FiscalID = "123"
                }
            };

            await billingService.CreateBillingRequest(billingRequestList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Never);
            slackServiceMock.Verify(x => x.SendNotification(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BillingService_ShouldBeAddOneTaskInQueue_WhenBillingRequestListHasOneInvalidCountryCodeElement()
        {
            var billingMappers = new List<IBillingMapper>
            {
                new BillingForArMapper(Mock.Of<ISapBillingItemsService>()),
                new BillingForUsMapper(Mock.Of<ISapBillingItemsService>())
            };

            var slackServiceMock = new Mock<ISlackService>();
            slackServiceMock.Setup(x => x.SendNotification(It.IsAny<string>())).Returns(Task.CompletedTask);

            var queuingServiceMock = new Mock<IQueuingService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            var billingService = new BillingService(queuingServiceMock.Object,
                dateTimeProviderMock.Object,
                Mock.Of<ILogger<BillingService>>(),
                slackServiceMock.Object,
                billingMappers,
                null);

            var billingRequestList = new List<BillingRequest>
            {
                new BillingRequest
                {
                    BillingSystemId = 16,
                    FiscalID = "123"
                }
            };

            await billingService.CreateBillingRequest(billingRequestList);

            queuingServiceMock.Verify(x => x.AddToTaskQueue(It.IsAny<SapTask>()), Times.Never);
            slackServiceMock.Verify(x => x.SendNotification(It.IsAny<string>()), Times.Once);
        }
    }
}
