using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MyPersonalNewspaper.Commands;
using MyPersonalNewspaper.Models;
using MyPersonalNewspaper.Services;

namespace MyPersonalNewspaper.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NewsService _newsService = new();
        private readonly CurrencyService _currencyService = new();
        private readonly DispatcherTimer _featuredTimer;
        private int _featuredIndex = 0;

        public ObservableCollection<NewsArticle> Articles { get; } = new();
        public ObservableCollection<NewsArticle> TodayArticles { get; } = new();
        public ObservableCollection<CurrencyRate> Currencies { get; } = new();

        public ObservableCollection<TopicItem> Topics { get; } = new()
        {
            new TopicItem { Name = "Общее", Query = "news" },
            new TopicItem { Name = "Игры", Query = "games" },
            new TopicItem { Name = "Политика", Query = "politics" },
            new TopicItem { Name = "Технологии", Query = "technology" },
            new TopicItem { Name = "Наука", Query = "science" },
            new TopicItem { Name = "Кино", Query = "movies" },
            new TopicItem { Name = "Спорт", Query = "sports" }
        };

        private NewsArticle _featuredArticle;
        public NewsArticle FeaturedArticle
        {
            get => _featuredArticle;
            set { _featuredArticle = value; OnPropertyChanged(); }
        }

        private NewsArticle _selectedArticle;
        public NewsArticle SelectedArticle
        {
            get => _selectedArticle;
            set { _selectedArticle = value; OnPropertyChanged(); }
        }

        private bool _isReadingMode;
        public bool IsReadingMode
        {
            get => _isReadingMode;
            set
            {
                _isReadingMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFeedVisible));
            }
        }

        public bool IsFeedVisible => !IsReadingMode;

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); }
        }

        public string MarqueeText
        {
            get
            {
                if (Articles == null || Articles.Count == 0)
                    return "СИСТЕМА: НЕТ НОВОСТЕЙ // ОЖИДАНИЕ ДАННЫХ...";
                return string.Join("  //  ", Articles.Select(a => a.Title)) + "  //  ";
            }
        }

        public ICommand OpenArticleCommand { get; }
        public ICommand BackToFeedCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand FilterCommand { get; }

        public MainViewModel()
        {
            OpenArticleCommand = new RelayCommand(OpenArticle);

            BackToFeedCommand = new RelayCommand(_ =>
            {
                IsReadingMode = false;
                _featuredTimer.Start();
            });

            SearchCommand = new RelayCommand(async _ => await SearchNewsAsync(SearchQuery));

            FilterCommand = new RelayCommand(async param =>
            {
                if (param is TopicItem topic)
                {
                    await SearchNewsAsync(topic.Query);
                }
            });

            Articles.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MarqueeText));

            _featuredTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _featuredTimer.Tick += OnFeaturedTimerTick;

            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            var rates = await _currencyService.GetRatesAsync();
            Currencies.Clear();
            foreach (var rate in rates.OrderBy(r => r.CharCode == "RUB" ? 1 : 0))
                Currencies.Add(rate);

            await SearchNewsAsync("news");
        }

        private async Task SearchNewsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            _featuredTimer.Stop();
            Debug.WriteLine($"[SEARCH] Запрос: {query}");

            var news = await _newsService.GetTopNewsAsync(query);

            Articles.Clear();
            foreach (var article in news)
                Articles.Add(article);

            if (TodayArticles.Count == 0 && Articles.Count > 0)
            {
                foreach (var a in Articles.Take(10)) TodayArticles.Add(a);
            }

            if (Articles.Count == 0)
            {
                FeaturedArticle = new NewsArticle
                {
                    Title = "НИЧЕГО НЕ НАЙДЕНО",
                    Description = $"По запросу \"{query}\" результатов нет.",
                    SourceName = "SYSTEM"
                };
                return;
            }

            _featuredIndex = 0;
            FeaturedArticle = Articles[0];
            _featuredTimer.Start();
        }

        private void OnFeaturedTimerTick(object sender, EventArgs e)
        {
            if (IsReadingMode || Articles.Count <= 1) return;
            _featuredIndex = (_featuredIndex + 1) % Articles.Count;
            FeaturedArticle = Articles[_featuredIndex];
        }

        private void OpenArticle(object parameter)
        {
            if (parameter is NewsArticle article)
            {
                SelectedArticle = article;
                IsReadingMode = true;
                _featuredTimer.Stop();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TopicItem
    {
        public string Name { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
    }
}