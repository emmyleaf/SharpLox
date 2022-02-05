using static SharpLox.TokenType;

namespace SharpLox;

public class Interpreter : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    private readonly Env environment = new();

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

    #region Statement Visitor Implementation

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    #endregion

    #region Expression Visitor Implementation

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
                if (left is double && right is double) {
                    return (double)left + (double)right;
                } 

                if (left is string && right is string) {
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

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
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
        return environment.Get(expr.Name);
    }

    #endregion

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
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

    private static string? Stringify(object? o) {
        if (o is null) return "nil";

        if (o is double n) {
            var text = n.ToString();
            if (text.EndsWith(".0")) {
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
