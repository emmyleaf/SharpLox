namespace SharpLox;

public class LoxClass : LoxCallable
{
    public readonly string Name;
    private readonly LoxClass? superclass;
    private readonly Dictionary<String, LoxFunction> methods;

    public int Arity => FindMethod("init")?.Arity ?? 0;

    public LoxClass(string name, LoxClass? superclass, Dictionary<String, LoxFunction> methods)
    {
        this.Name = name;
        this.superclass = superclass;
        this.methods = methods;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);

        if (FindMethod("init") is LoxFunction initializer)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public LoxFunction? FindMethod(String name)
    {
        if (methods.TryGetValue(name, out var method))
        {
            return method;
        }

        return superclass?.FindMethod(name);
    }

    public override string ToString() => Name;
}
