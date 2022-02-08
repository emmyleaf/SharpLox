namespace SharpLox;

public class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly Env closure;

    public LoxFunction(Stmt.Function declaration, Env closure)
    {
        this.declaration = declaration;
        this.closure = closure;
    }

    public int Arity => declaration.Parameters.Count;

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Env(closure);
        environment.Define("this", instance);
        return new LoxFunction(declaration, environment);
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
            return returnValue.Value;
        }
        return null;
    }

    public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
}
