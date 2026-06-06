using MathGame.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MathGame.Views;

public partial class OperationSelectView : UserControl
{
    private OperationSelectViewModel? _vm;

    public OperationSelectView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = e.NewValue as OperationSelectViewModel;
        if (_vm is not null)
        {
            _vm.PropertyChanged += OnVmPropertyChanged;
            if (_vm.DifficultyConfirmed)
            {
                DifficultyPanel.Visibility = Visibility.Collapsed;
                GamePanel.Visibility       = Visibility.Visible;
            }
            RefreshCompleteOverlays();
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OperationSelectViewModel.DifficultyConfirmed)
            && _vm?.DifficultyConfirmed == true)
        {
            AnimateDifficultyOut();
            return;
        }
        if (e.PropertyName is nameof(OperationSelectViewModel.IsHorseComplete)
                           or nameof(OperationSelectViewModel.IsDolphinComplete)
                           or nameof(OperationSelectViewModel.IsTigerComplete)
                           or nameof(OperationSelectViewModel.IsDragonComplete))
        {
            RefreshCompleteOverlays();
        }
    }

    private void RefreshCompleteOverlays()
    {
        if (_vm is null) return;
        UpdateOverlay(_vm.IsHorseComplete,   HorseCompleteOverlay,   HorseGoldBorder,   HorseCompleteScale);
        UpdateOverlay(_vm.IsDolphinComplete, DolphinCompleteOverlay, DolphinGoldBorder, DolphinCompleteScale);
        UpdateOverlay(_vm.IsTigerComplete,   TigerCompleteOverlay,   TigerGoldBorder,   TigerCompleteScale);
        UpdateOverlay(_vm.IsDragonComplete,  DragonCompleteOverlay,  DragonGoldBorder,  DragonCompleteScale);
    }

    private static void UpdateOverlay(bool isComplete, Grid overlay, Border goldBorder, ScaleTransform scale)
    {
        if (isComplete)
            ShowComplete(overlay, goldBorder, scale);
        else
            HideComplete(overlay, goldBorder, scale);
    }

    private static void ShowComplete(Grid overlay, Border goldBorder, ScaleTransform scale)
    {
        overlay.Visibility = Visibility.Visible;
        scale.ScaleX = 0;
        scale.ScaleY = 0;

        var easing = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 5 };
        var dur     = new Duration(TimeSpan.FromSeconds(0.5));

        var sx = new DoubleAnimation(0, 1, dur) { EasingFunction = easing };
        Storyboard.SetTarget(sx, scale);
        Storyboard.SetTargetProperty(sx, new PropertyPath(ScaleTransform.ScaleXProperty));

        var sy = new DoubleAnimation(0, 1, dur) { EasingFunction = easing };
        Storyboard.SetTarget(sy, scale);
        Storyboard.SetTargetProperty(sy, new PropertyPath(ScaleTransform.ScaleYProperty));

        var bounce = new Storyboard();
        bounce.Children.Add(sx);
        bounce.Children.Add(sy);
        bounce.Begin();

        var pulse = new DoubleAnimation(0.3, 1.0, new Duration(TimeSpan.FromSeconds(0.9)))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        goldBorder.BeginAnimation(OpacityProperty, pulse);
    }

    private static void HideComplete(Grid overlay, Border goldBorder, ScaleTransform scale)
    {
        goldBorder.BeginAnimation(OpacityProperty, null);
        overlay.Visibility = Visibility.Collapsed;
        scale.ScaleX = 0;
        scale.ScaleY = 0;
    }

    private void AnimateDifficultyOut()
    {
        var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.28)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            DifficultyPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Visible;
            GamePanel.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.4)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            GamePanel.BeginAnimation(OpacityProperty, fadeIn);
            RefreshCompleteOverlays();
        };
        DifficultyPanel.BeginAnimation(OpacityProperty, fadeOut);
    }
}
