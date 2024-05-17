using CurrencyConverterApp.DTOs;

namespace CurrencyConverterApp.Service
{
    public interface ICurrencyConverterService
    {
        Task<CurrencyRatesLatest?> GetLatestConversionsFromBaseCurrency(string baseCurrencyCode);

        Task<CurrencyRatesLatest?> ConvertAmountFromAndToCurrencies(decimal amount,string fromCurrency, string toCurrencies);

        Task<CurrencyRatesHistoric?> GetHistoricConversionsFromBaseCurrency(string baseCurrencyCode, string dateRange);

        bool ValidateUnsupportedCurrencies(string baseCurrency);
    }
}
