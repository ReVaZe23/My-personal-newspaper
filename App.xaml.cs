using System.Windows;

namespace MyPersonalNewspaper
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Важно: не даем приложению закрыться, когда SplashWindow закроется
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var splash = new SplashWindow();
            splash.Show();

            splash.Closed += (s, args) =>
            {
                // Создаем и показываем главное окно
                var mainWindow = new MainWindow();
                MainWindow = mainWindow; // Назначаем его главным окном приложения
                mainWindow.Show();

                // После того как главное окно показано, возвращаем обычный режим завершения
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            };
        }
    }
}