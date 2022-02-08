namespace SharpLox;

public class Resolver : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private ClassType currentClass = ClassType.NONE;
    private FunctionType currentFunction = FunctionType.NONE;

    public Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
    {
        statements.ForEach(Resolve);
    }

    #region Statement Visitor Implementation

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = currentClass;
        currentClass = ClassType.CLASS;

        Declare(stmt.Name);
        Define(stmt.Name);

        BeginScope();
        scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            ResolveFunction(method, FunctionType.METHOD);
        }

        EndScope();

        currentClass = enclosingClass;
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch is not null) Resolve(stmt.ElseBranch);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            Lox.Error(stmt.Keyword, "Can't return from top-level code.");
        }
        if (stmt.Value is not null) Resolve(stmt.Value);
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer is not null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    #endregion

    #region Expression Visitor Implementation

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);
        expr.Arguments.ForEach(Resolve);
        return null;
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        if (currentClass == ClassType.NONE)
        {
            Lox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (scopes.TryPeek(out var scope))
        {
            if (scope.TryGetValue(expr.Name.Lexeme, out var defined) && !defined)
            {
                Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
            }
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    #endregion

    private void BeginScope() => scopes.Push(new());
    private void EndScope() => scopes.Pop();

    private void Declare(Token name)
    {
        if (scopes.TryPeek(out var scope))
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }
            scope[name.Lexeme] = false;
        }
    }

    private void Define(Token name)
    {
        if (scopes.TryPeek(out var scope))
        {
            scope[name.Lexeme] = true;
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = currentFunction;
        currentFunction = type;
        BeginScope();
        foreach (var parameter in function.Parameters)
        {
            Declare(parameter);
            Define(parameter);
        }
        Resolve(function.Body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = 0; i < scopes.Count; i++)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void Resolve(Stmt stmt) => stmt.Accept(this);
    private void Resolve(Expr expr) => expr.Accept(this);

    private enum ClassType
    {
        NONE,
        CLASS,
    }

    private enum FunctionType
    {
        NONE,
        FUNCTION,
        METHOD,
    }
}
