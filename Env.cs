namespace SharpLox;

public class Env
{
    public readonly Env? Enclosing;
    private readonly Dictionary<string, object?> Values = new();

    public Env(Env? enclosing = null)
    {
        Enclosing = enclosing;
    }

    public void Assign(Token name, object? value)
    {
        if (Values.TryAdd(name.Lexeme, value))
        {
            return;
        }

        if (Enclosing is not null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Define(string name, object? value)
    {
        Values.Add(name, value);
    }

    public object? Get(Token name)
    {
        if (Values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (Enclosing is not null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
