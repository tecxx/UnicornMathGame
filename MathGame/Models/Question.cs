using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MathGame.Models;

public class Question : INotifyPropertyChanged
{
    public int Operand1 { get; }
    public int Operand2 { get; }
    public Operation Op { get; }
    public int Answer { get; }

    private QuestionState _state;
    public QuestionState State
    {
        get => _state;
        set { _state = value; OnPropertyChanged(); }
    }

    public string QuestionText => Op switch
    {
        Operation.Addition       => $"{Operand1} + {Operand2}",
        Operation.Subtraction    => $"{Operand1} - {Operand2}",
        Operation.Multiplication => $"{Operand1} × {Operand2}",
        Operation.Division       => $"{Operand1} ÷ {Operand2}",
        _                        => ""
    };

    public Question(int op1, int op2, Operation operation, int answer)
    {
        Operand1 = op1;
        Operand2 = op2;
        Op = operation;
        Answer = answer;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
