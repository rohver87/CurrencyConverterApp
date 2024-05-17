using CurrencyConverterApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApp.Controllers
{
    [ApiController]
    [Route("currencyconverter")]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ILogger<CurrencyConverterController> _logger;
        private readonly ICurrencyConverterService _service;

        public CurrencyConverterController(ILogger<CurrencyConverterController> logger, ICurrencyConverterService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("exchange-rates")]
        public async Task<IActionResult> GetLatestConversionsFromBaseCurrency(string baseCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
            {
                return BadRequest();
            }

            var currencyRates = await _service.GetLatestConversionsFromBaseCurrency(baseCurrency);

            return Ok(currencyRates);
        }

        [HttpGet("amount-conversions")]
        public async Task<IActionResult> GetLatestConversionsFromBaseCurrency(decimal amount, string baseCurrency, string toCurrencies)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(toCurrencies))
            {
                return BadRequest();
            }

            if (_service.ValidateUnsupportedCurrencies(baseCurrency)) return BadRequest();

            var currencyRates = await _service.ConvertAmountFromAndToCurrencies(amount,baseCurrency, toCurrencies);
            
            return Ok(currencyRates);
        }

        [HttpGet("historic-rates")]
        public async Task<IActionResult> GetHistoricConversionsFromBaseCurrency(string baseCurrency, string dateRange)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(dateRange))
            {
                return BadRequest();
            }

            var currencyRates = await _service.GetHistoricConversionsFromBaseCurrency(baseCurrency,dateRange);

            return Ok(currencyRates);
        }
    }
}
