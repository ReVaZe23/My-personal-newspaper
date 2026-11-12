using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MyPersonalNewspaper
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            LoadBackground();
            SetGreeting();
            StartLoadingAnimation();
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

                // Присваиваем картинку в Image внутри обрезанного Grid
                BackgroundImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фона Splash: {ex.Message}");
            }
        }

        private void SetGreeting()
        {
            string userName = Environment.UserName;
            UserNameText.Text = $">>> {userName.ToUpper()} <<<";

            int hour = DateTime.Now.Hour;
            string greeting = "Доброй ночи";

            if (hour >= 5 && hour < 12) greeting = "Доброе утро";
            else if (hour >= 12 && hour < 18) greeting = "Добрый день";
            else if (hour >= 18 && hour < 23) greeting = "Добрый вечер";

            GreetingText.Text = $"{greeting},";
        }

        private void StartLoadingAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = TimeSpan.FromSeconds(2.5)
            };

            animation.Completed += (s, e) => this.Close();
            LoadBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation);
        }
    }
}