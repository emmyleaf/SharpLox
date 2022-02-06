namespace SharpLox;

public interface Expr
{
    public interface Visitor<T>
    {
        T VisitAssignExpr(Assign expr);
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
        T VisitVariableExpr(Variable expr);
    }

    T Accept<T>(Visitor<T> visitor);

    public record Assign(Token Name, Expr Value) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitAssignExpr(this);
    }

    public record Binary(Expr Left, Token Operator, Expr Right) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
    }

    public record Grouping(Expr Expression) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
    }

    public record Literal(object? Value) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
    }

    public record Unary(Token Operator, Expr Right) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
    }

    public record Variable(Token Name) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitVariableExpr(this);
    }
}
