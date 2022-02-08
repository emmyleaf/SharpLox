namespace SharpLox;

public class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly Env closure;
    private readonly bool isInitializer;

    public LoxFunction(Stmt.Function declaration, Env closure, bool isInitializer)
    {
        this.declaration = declaration;
        this.closure = closure;
        this.isInitializer = isInitializer;
    }

    public int Arity => declaration.Parameters.Count;

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Env(closure);
        environment.Define("this", instance);
        return new LoxFunction(declaration, environment, isInitializer);
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Env(closure);
        for (int i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (isInitializer) return closure.GetAt(0, "this");
            return returnValue.Value;
        }

        if (isInitializer) return closure.GetAt(0, "this");
        return null;
    }

    public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
}
