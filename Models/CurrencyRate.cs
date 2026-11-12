namespace MyPersonalNewspaper.Models
{
    public class CurrencyRate
    {
        public string Name { get; set; } = string.Empty;
        public string CharCode { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string DisplayValue => $"{Value:F2} ₽";
    }
}