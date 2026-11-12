using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using MyPersonalNewspaper.ViewModels;

namespace MyPersonalNewspaper
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly DispatcherTimer _autoScrollTimer;

        public MainWindow()
        {
            InitializeComponent();
            LoadBackground();
            InitializeWebView();

            _viewModel = (MainViewModel)DataContext;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            _autoScrollTimer = new DispatcherTimer();
            _autoScrollTimer.Interval = TimeSpan.FromMilliseconds(50);
            _autoScrollTimer.Tick += AutoScrollTimer_Tick;
            _autoScrollTimer.Start();
        }

        private void LoadBackground()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDir, "Assets", "Background.jpg");

                if (!File.Exists(fullPath)) return;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                RootBorder.Background = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 1.0
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фона: {ex.Message}");
            }
        }

        private async void InitializeWebView()
        {
            await ArticleBrowser.EnsureCoreWebView2Async();
            ArticleBrowser.DefaultBackgroundColor = System.Drawing.Color.Transparent;
        }

        private void AutoScrollTimer_Tick(object sender, EventArgs e)
        {
            if (NewsScroller == null) return;
            if (NewsScroller.HorizontalOffset >= NewsScroller.ScrollableWidth)
                NewsScroller.ScrollToHorizontalOffset(0);
            else
                NewsScroller.ScrollToHorizontalOffset(NewsScroller.HorizontalOffset + 1.5);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsReadingMode)
                && _viewModel.IsReadingMode
                && !string.IsNullOrEmpty(_viewModel.SelectedArticle?.Url))
            {
                ArticleBrowser.NavigationCompleted -= OnNavigationCompleted;
                ArticleBrowser.NavigationCompleted += OnNavigationCompleted;
                ArticleBrowser.Source = new Uri(_viewModel.SelectedArticle.Url);
            }
        }

        private async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;
            string cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "inject.css");
            if (!File.Exists(cssPath)) return;

            string css = await File.ReadAllTextAsync(cssPath);
            string escapedCss = JsonSerializer.Serialize(css);
            string script = $@"(function() {{ var style = document.createElement('style'); style.textContent = {escapedCss}; (document.head || document.documentElement).appendChild(style); }})();";
            await ArticleBrowser.CoreWebView2.ExecuteScriptAsync(script);
        }

        private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}