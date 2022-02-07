namespace SharpLox;

public interface LoxCallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object?> arguments);

    public class Clock : LoxCallable
    {
        public int Arity => 0;

        public object? Call(Interpreter _i, List<object?> _a)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString() => "<native fn>";
    }
}
