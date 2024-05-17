namespace CurrencyConverterApp.DTOs
{
    public class CurrencyRatesHistoric
    {
        public decimal Amount { get; set; }

        public string Base { get; set; }

        public string Start_date { get; set; }

        public string End_date { get; set; }

        public Dictionary<string, Dictionary<string,decimal>> Rates { get; set; }
    }
}
