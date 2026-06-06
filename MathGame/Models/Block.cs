namespace MathGame.Models;

public class Block
{
    public int Number { get; }
    public Operation Op { get; }
    public List<Question> Questions { get; }

    public Block(int number, Operation op, List<Question> questions)
    {
        Number = number;
        Op = op;
        Questions = questions;
    }

    private IEnumerable<Question> Active => Questions.Where(q => q.State != QuestionState.Disabled);

    public bool IsCompleted   => Active.Any() && Active.All(q => q.State == QuestionState.Correct);
    public bool IsAllAnswered => Active.All(q => q.State is QuestionState.Correct or QuestionState.Wrong);
    public int  CorrectCount  => Active.Count(q => q.State == QuestionState.Correct);
    public int  SolvedCount   => Active.Count(q => q.State is QuestionState.Correct or QuestionState.Wrong);
}
