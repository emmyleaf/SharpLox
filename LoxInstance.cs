namespace SharpLox;

public class LoxInstance
{
    private LoxClass klass;
    private readonly Dictionary<string, object?> fields = new();

    public LoxInstance(LoxClass klass)
    {
        this.klass = klass;
    }

    public object? Get(Token name)
    {
        if (fields.TryGetValue(name.Lexeme, out var field))
        {
            return field;
        }

        var method = klass.FindMethod(name.Lexeme);
        if (method is not null) return method;

        throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
    }

    public void Set(Token name, object? value)
    {
        fields[name.Lexeme] = value;
    }

    public override string ToString() => $"{klass.Name} instance";
}
