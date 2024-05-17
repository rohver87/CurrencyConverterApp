using CurrencyConverterApp.DTOs;
using CurrencyConverterApp.Service;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;

namespace CurrencyConverterApp.Tests
{
    public class CurrencyConverterServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientMock = new();
        private readonly Mock<IConfiguration> _config = new();
        private readonly Mock<HttpMessageHandler> _httpMessageHandler = new();
        public CurrencyConverterServiceTests()
        {
            var configurationSectionMock1 = new Mock<IConfigurationSection>();
            var configurationSectionMock2 = new Mock<IConfigurationSection>();

            configurationSectionMock1
               .Setup(x => x.Value)
               .Returns("CurrencyConverterApiClient");

            configurationSectionMock2
              .Setup(x => x.Value)
              .Returns("TRY,PLN,THB,MXN");

            _config
               .Setup(x => x.GetSection("CurrencyConverterApiClient"))
               .Returns(configurationSectionMock1.Object);

            _config
              .Setup(x => x.GetSection("UnsupportedCurrencies"))
              .Returns(configurationSectionMock2.Object);
        }

        [Fact]
        public async Task CurrencyConverterServiceTests_GetLatestConversionsFromBaseCurrency_Success()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpMessageHandler
                .SetupSendAsync(HttpMethod.Get, $"{fakeBaseAddress}/latest?from=usd")
                .ReturnsHttpResponseAsync(new CurrencyRatesLatest()
                {
                    Amount = 1,
                    Date = "2024-05-16",
                    Rates = new Dictionary<string, decimal>()
                }
                , HttpStatusCode.OK);

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = await sut.GetLatestConversionsFromBaseCurrency("usd");

            result.Should().NotBeNull();
            result.Amount.Should().Be(1);
        }

        [Fact]
        public async Task CurrencyConverterServiceTests_ConvertAmountFromAndToCurrencies_WithAmount_Success()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpMessageHandler
                .SetupSendAsync(HttpMethod.Get, $"{fakeBaseAddress}/latest?amount=10&from=usd&to=inr")
                .ReturnsHttpResponseAsync(new CurrencyRatesLatest()
                {
                    Amount = 10,
                    Date = "2024-05-16",
                    Rates = new Dictionary<string, decimal>() { {"inr",83 } }
                }
                , HttpStatusCode.OK);

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = await sut.ConvertAmountFromAndToCurrencies(10,"usd","inr");

            result.Should().NotBeNull();
            result.Rates["inr"].Should().Be(83);
        }

        [Fact]
        public async Task CurrencyConverterServiceTests_ConvertAmountFromAndToCurrencies_WithoutAmount_Success()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpMessageHandler
                .SetupSendAsync(HttpMethod.Get, $"{fakeBaseAddress}/latest?from=usd&to=inr")
                .ReturnsHttpResponseAsync(new CurrencyRatesLatest()
                {
                    Amount = 1,
                    Date = "2024-05-16",
                    Rates = new Dictionary<string, decimal>() { { "inr", 83 } }
                }
                , HttpStatusCode.OK);

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = await sut.ConvertAmountFromAndToCurrencies(0, "usd", "inr");

            result.Should().NotBeNull();
            result.Rates["inr"].Should().Be(83);
        }

        [Fact]
        public async Task CurrencyConverterServiceTests_GetHistoricConversionsFromBaseCurrency_Success()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpMessageHandler
                .SetupSendAsync(HttpMethod.Get, $"{fakeBaseAddress}/2020-01-01..2020-01-2?from=usd")
                .ReturnsHttpResponseAsync(new CurrencyRatesHistoric()
                {
                    Amount = 1,
                    Start_date = "2020-01-01",
                    End_date = "2020-01-2",
                    Rates = new Dictionary<string, Dictionary<string,decimal>> { 
                        { "2020-01-01", new Dictionary<string, decimal>() },
                        { "2020-01-02", new Dictionary<string, decimal>() }
                    }
                }
                , HttpStatusCode.OK);

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = await sut.GetHistoricConversionsFromBaseCurrency("usd", "2020-01-01..2020-01-2");

            result.Should().NotBeNull();
            result.Rates.Count.Should().Be(2);
        }

        [Fact]
        public void CurrencyConverterServiceTests_ValidateUnsupportedCurrencies_Invalid()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = sut.ValidateUnsupportedCurrencies("try");

            result.Should().BeTrue();
        }

        [Fact]
        public void CurrencyConverterServiceTests_ValidateUnsupportedCurrencies_Valid()
        {
            var fakeBaseAddress = "https://www.unittests.com";

            _httpClientMock.Setup(x => x.CreateClient("CurrencyConverterApiClient"))
                           .Returns(new HttpClient(_httpMessageHandler.Object)
                           {
                               BaseAddress = new Uri(fakeBaseAddress)
                           });

            var sut = new CurrencyConverterService(_httpClientMock.Object, _config.Object);

            var result = sut.ValidateUnsupportedCurrencies("usd");

            result.Should().BeFalse();
        }
    }
}