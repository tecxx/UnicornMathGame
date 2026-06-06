using MathGame.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MathGame.Views;

public partial class GameView : UserControl
{
    private GameViewModel? _vm;

    public GameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        IsVisibleChanged   += (_, _) => FocusAnswerBox();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = e.NewValue as GameViewModel;
        if (_vm is not null) _vm.PropertyChanged += OnVmPropertyChanged;
        FocusAnswerBox();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameViewModel.IsGameWon) && _vm?.IsGameWon == true)
            AnimateWinCard();
    }

    private void AnimateWinCard()
    {
        var easing = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 5 };
        var dur    = new Duration(TimeSpan.FromSeconds(0.55));

        var animX = new DoubleAnimation(0.3, 1.0, dur) { EasingFunction = easing };
        var animY = new DoubleAnimation(0.3, 1.0, dur) { EasingFunction = easing };

        WinCardScale.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
        WinCardScale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
    }

    private void FocusAnswerBox()
    {
        if (DataContext is GameViewModel)
            Dispatcher.InvokeAsync(() => AnswerBox.Focus(),
                System.Windows.Threading.DispatcherPriority.Input);
    }
}
