namespace SharpLox;

public class Env
{
    public readonly Env? Enclosing;
    private readonly Dictionary<string, object?> Values = new();

    public Env(Env? enclosing = null)
    {
        Enclosing = enclosing;
    }

    public Env Ancestor(int distance)
    {
        var environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.Enclosing!; // we just gotta trust
        }
        return environment;
    }

    public void Assign(Token name, object? value)
    {
        // This was wrong in previous commit. TryAdd is definitely not equivalent to this!
        if (Values.ContainsKey(name.Lexeme))
        {
            Values[name.Lexeme] = value;
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

    public object? GetAt(int distance, String name)
    {
        return Ancestor(distance).Values[name];
    }
}
