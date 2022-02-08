using System.Text;

namespace SharpLox;

public class AstPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr) => expr.Accept(this);

    public string VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitCallExpr(Expr.Call expr)
    {
        throw new NotImplementedException();
    }

    public string VisitGetExpr(Expr.Get expr)
    {
        throw new NotImplementedException();
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value?.ToString() ?? "nil";
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    public string VisitSetExpr(Expr.Set expr)
    {
        throw new NotImplementedException();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        foreach (var expr in exprs)
        {
            builder.Append(' ').Append(expr.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }
}
