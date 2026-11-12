using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MyPersonalNewspaper.Models;

namespace MyPersonalNewspaper.Services
{
    public class CurrencyService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<CurrencyRate>> GetRatesAsync()
        {
            var result = new List<CurrencyRate>();
            try
            {
                // Запрос к бесплатному API. База - USD.
                string url = "https://cdn.jsdelivr.net/gh/irfanokr/currency-api@main/v1/currencies/usd.json";
                var json = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Ищем секцию "usd" (базовая валюта)
                if (root.TryGetProperty("usd", out var usdRates))
                {
                    // Получаем курс рубля к доллару (сколько рублей за 1 доллар)
                    decimal rubRate = usdRates.GetProperty("rub").GetDecimal();
                    // Получаем курс евро к доллару (сколько евро за 1 доллар)
                    decimal eurRate = usdRates.GetProperty("eur").GetDecimal();

                    // Пересчитываем: сколько РУБЛЕЙ за 1 ЕВРО
                    // Если 1 USD = X RUB, и 1 USD = Y EUR, то 1 EUR = X / Y RUB
                    decimal eurToRub = rubRate / eurRate;

                    // Добавляем в список (рубль всегда 1)
                    result.Add(new CurrencyRate { Name = "Доллар США", CharCode = "USD", Value = rubRate });
                    result.Add(new CurrencyRate { Name = "Евро", CharCode = "EUR", Value = eurToRub });
                    result.Add(new CurrencyRate { Name = "Российский рубль", CharCode = "RUB", Value = 1.0m });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Currency API Error: {ex.Message}");
                // Заглушки при ошибке
                result.Add(new CurrencyRate { Name = "Доллар США", CharCode = "USD", Value = 0 });
                result.Add(new CurrencyRate { Name = "Евро", CharCode = "EUR", Value = 0 });
                result.Add(new CurrencyRate { Name = "Российский рубль", CharCode = "RUB", Value = 1 });
            }
            return result;
        }
    }
}