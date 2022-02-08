namespace SharpLox;

public class LoxClass : LoxCallable
{
    public readonly string Name;
    private readonly Dictionary<String, LoxFunction> methods;

    public int Arity => 0;

    public LoxClass(string name, Dictionary<String, LoxFunction> methods)
    {
        this.Name = name;
        this.methods = methods;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        return instance;
    }

    public LoxFunction? FindMethod(String name)
    {
        return methods.TryGetValue(name, out var method) ? method : null;
    }

    public override string ToString() => Name;
}
