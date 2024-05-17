using CurrencyConverterApp.DTOs;
using System.Text.Json;

namespace CurrencyConverterApp.Service
{
    public class CurrencyConverterService:ICurrencyConverterService
    {
        private readonly IHttpClientFactory _httpClientFactory = null!;
        private readonly IConfiguration _configuration = null!;
        private readonly string _currencyConverterApiClient = null!;

        public CurrencyConverterService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _currencyConverterApiClient = configuration.GetValue<string>("CurrencyConverterApiClient");
        }

        public async Task<CurrencyRatesLatest?> GetLatestConversionsFromBaseCurrency(string baseCurrencyCode)
        {
            // Create the client           
            using HttpClient client = _httpClientFactory.CreateClient(_currencyConverterApiClient ?? "");

            var currencyRates = await client.GetFromJsonAsync<CurrencyRatesLatest>(
               $"latest?from={baseCurrencyCode}",
               new JsonSerializerOptions(JsonSerializerDefaults.Web));

            return currencyRates;
        }

        public async Task<CurrencyRatesLatest?> ConvertAmountFromAndToCurrencies(decimal amount,string fromCurrency,string toCurrencies)
        {       
            using HttpClient client = _httpClientFactory.CreateClient(_currencyConverterApiClient ?? "");

            string relativePath = string.Empty;

            var toCurrenciesUpdated = RemoveUnsupportedCurrencyFromList(toCurrencies);

            if (amount>1)
            {
                relativePath = $"latest?amount={amount}&from={fromCurrency}&to={toCurrenciesUpdated}";              
            }
            else
            {
                relativePath = $"latest?from={fromCurrency}&to={toCurrenciesUpdated}";
            }

            var currencyRates = await client.GetFromJsonAsync<CurrencyRatesLatest>(relativePath,
               new JsonSerializerOptions(JsonSerializerDefaults.Web));

            return currencyRates;
        }

        private string RemoveUnsupportedCurrencyFromList(string toCurrencies)
        {
            var unsupportedCurrencyList = _configuration.GetValue<string>("UnsupportedCurrencies");

            var returnList = new List<string>();

            foreach(var currency in toCurrencies.Split(","))
            {
                if (!unsupportedCurrencyList.Contains(currency,StringComparison.OrdinalIgnoreCase))
                {
                    returnList.Add(currency);
                }
            }

            return string.Join(",", returnList);
        }

        public bool ValidateUnsupportedCurrencies(string baseCurrency)
        {
            var unsupportedCurrencyList = _configuration.GetValue<string>("UnsupportedCurrencies");

            if (unsupportedCurrencyList.IndexOf(baseCurrency.ToUpperInvariant())>-1)
            {
                return true;
            }

            return false;
        }

        public async Task<CurrencyRatesHistoric?> GetHistoricConversionsFromBaseCurrency(string baseCurrencyCode,string dateRange)
        {
            // Create the client           
            using HttpClient client = _httpClientFactory.CreateClient(_currencyConverterApiClient ?? "");

            var currencyRates = await client.GetFromJsonAsync<CurrencyRatesHistoric>(
               $"{dateRange}?from={baseCurrencyCode}",
               new JsonSerializerOptions(JsonSerializerDefaults.Web));

            return currencyRates;
        }
    }
}
