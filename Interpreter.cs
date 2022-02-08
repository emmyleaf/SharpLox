using static SharpLox.TokenType;

namespace SharpLox;

public class Interpreter : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    public readonly Env Globals;
    private Env environment;
    private readonly Dictionary<Expr, int> locals;

    public Interpreter()
    {
        Globals = new();
        environment = Globals;
        locals = new();
        Globals.Define("clock", new LoxCallable.Clock());
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            statements.ForEach(Execute);
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    public void Resolve(Expr expr, int depth) => locals[expr] = depth;

    #region Statement Visitor Implementation

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Env(environment));
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        LoxClass? superclass = null;
        if (stmt.Superclass is not null)
        {
            superclass = Evaluate(stmt.Superclass) as LoxClass;
            if (superclass is null)
            {
                throw new RuntimeError(stmt.Superclass.Name, "Superclass must be a class.");
            }
        }

        environment.Define(stmt.Name.Lexeme, null);

        if (stmt.Superclass is not null)
        {
            environment = new Env(environment);
            environment.Define("super", superclass);
        }

        Dictionary<String, LoxFunction> methods = new();
        foreach (var method in stmt.Methods)
        {
            var isInitializer = method.Name.Lexeme == "init";
            var function = new LoxFunction(method, environment, isInitializer);
            methods[method.Name.Lexeme] = function;
        }

        var klass = new LoxClass(stmt.Name.Lexeme, superclass, methods);

        if (stmt.Superclass is not null)
        {
            environment = environment.Enclosing!;
        }

        environment.Assign(stmt.Name, klass);
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt, environment, false);
        environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (Truthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch is not null)
        {
            Execute(stmt.ElseBranch);
        }
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        var value = stmt.Value is null ? null : Evaluate(stmt.Value);
        throw new Return(value);
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer is not null)
        {
            value = Evaluate(stmt.Initializer);
        }

        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (Truthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }
        return null;
    }

    #endregion

    #region Expression Visitor Implementation

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        environment.Assign(expr.Name, value);
        return value;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case BANG_EQUAL:
                return !object.Equals(left, right);
            case EQUAL_EQUAL:
                return object.Equals(left, right);
            case GREATER:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! > (double)right!;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! >= (double)right!;
            case LESS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! < (double)right!;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! <= (double)right!;
            case MINUS:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! - (double)right!;
            case PLUS:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }

                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }

                throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            case SLASH:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! / (double)right!;
            case STAR:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! * (double)right!;
            default: return null;
        };
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        var function = Evaluate(expr.Callee) as LoxCallable;
        var arguments = expr.Arguments.ConvertAll(Evaluate);

        if (function is null)
        {
            throw new RuntimeError(expr.Paren, "Can only call function and classes.");
        }

        if (arguments.Count != function.Arity)
        {
            var message = $"Expected {function.Arity} arguments but got {arguments.Count}.";
            throw new RuntimeError(expr.Paren, message);
        }

        return function.Call(this, arguments);
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj is LoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError(expr.Name, "Only instances have properties.");
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == OR)
        {
            if (Truthy(left)) return left;
        }
        else
        {
            if (!Truthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        var obj = Evaluate(expr.Object);
        if (obj is not LoxInstance instance)
        {
            throw new RuntimeError(expr.Name, "Only instances have fields.");
        }

        var value = Evaluate(expr.Value);
        instance.Set(expr.Name, value);
        return value;
    }

    public object? VisitSuperExpr(Expr.Super expr)
    {
        // extremely yolo with nullability by this point
        var distance = locals.GetValueOrDefault(expr);
        var superclass = environment.GetAt(distance, "super") as LoxClass;
        var instance = environment.GetAt(distance - 1, "this") as LoxInstance;
        var method = superclass?.FindMethod(expr.Method.Lexeme);

        if (method is null)
        {
            throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.Lexeme}'.");
        }

        return method.Bind(instance!);
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case BANG:
                return !Truthy(right);
            case MINUS:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right!;
            default:
                return null;
        };
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    #endregion

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, Env blockEnv)
    {
        var previous = environment;
        try
        {
            environment = blockEnv;
            statements.ForEach(Execute);
        }
        finally
        {
            environment = previous;
        }
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (locals.TryGetValue(expr, out var distance))
        {
            return environment.GetAt(distance, name.Lexeme);
        }

        return Globals.Get(name);
    }

    private static void CheckNumberOperand(Token op, object? operand)
    {
        if (operand is not double)
        {
            throw new RuntimeError(op, "Operand must be a number.");
        }
    }

    private static void CheckNumberOperands(Token op, object? left, object? right)
    {
        if (left is not double || right is not double)
        {
            throw new RuntimeError(op, "Operands must be numbers.");
        }
    }

    private static string? Stringify(object? o)
    {
        if (o is null) return "nil";

        if (o is double n)
        {
            var text = n.ToString();
            if (text.EndsWith(".0"))
            {
                return text[0..(text.Length - 2)];
            }
            return text;
        }

        return o.ToString();
    }

    private static bool Truthy(object? o) => o switch
    {
        null => false,
        bool b => b,
        _ => true
    };
}
