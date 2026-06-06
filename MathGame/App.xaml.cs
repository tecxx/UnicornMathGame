using MathGame.ViewModels;
using MathGame.Views;
using System.Windows;

namespace MathGame;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var window = new MainWindow
        {
            DataContext = new MainViewModel()
        };
        window.Show();
    }
}
