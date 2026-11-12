using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MyPersonalNewspaper.Models;

namespace MyPersonalNewspaper.Services
{
    public class NewsService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiKey = "fna_live_L0x9Z3WuWRMXxTmvsPnf4dlwojQGP6BJ3cRYc-MOJUQ";

        public async Task<List<NewsArticle>> GetTopNewsAsync(string query = "news")
        {
            var articles = new List<NewsArticle>();
            try
            {
                string url = $"https://freenewsapi.ai/v1/search?q={Uri.EscapeDataString(query)}&lang=ru&size=50&sort=date";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {ApiKey}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("results", out var resultsArray))
                {
                    foreach (var item in resultsArray.EnumerateArray())
                    {
                        articles.Add(new NewsArticle
                        {
                            Title = item.GetProperty("title").GetString() ?? "",
                            Description = item.GetProperty("description").GetString() ?? "",
                            Url = item.GetProperty("url").GetString() ?? "",
                            ImageUrl = "", // Оставляем пустым, картинки не грузим
                            SourceName = item.GetProperty("sitename").GetString() ?? "",
                            PublishedAt = item.GetProperty("published_at").GetString()?.Substring(0, 10) ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"News API Error: {ex.Message}");
            }
            return articles;
        }
    }
}