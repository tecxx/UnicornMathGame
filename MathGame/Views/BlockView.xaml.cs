using MathGame.ViewModels;
using System.Windows.Controls;

namespace MathGame.Views;

public partial class BlockView : UserControl
{
    public BlockView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => FocusAnswerBox();
        IsVisibleChanged   += (_, _) => FocusAnswerBox();
    }

    private void FocusAnswerBox()
    {
        if (DataContext is BlockViewModel { IsCompleted: false })
            Dispatcher.InvokeAsync(() => AnswerBox.Focus(),
                System.Windows.Threading.DispatcherPriority.Input);
    }
}
