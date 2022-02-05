namespace SharpLox;

public class Env
{
    private readonly Dictionary<string, object?> Values = new();

    public void Define(string name, object? value)
    {
        Values.Add(name, value);
    }

    public object? Get(Token name)
    {
        if(Values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
