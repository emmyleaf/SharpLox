namespace SharpLox;

public interface Expr
{
    public interface Visitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
    }

    T Accept<T>(Visitor<T> visitor);

    public record Binary(Expr Left, Token Operator, Expr Right) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
    }

    public record Grouping(Expr Expression) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
    }

    public record Literal(object Value) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
    }

    public record Unary(Token Operator, Expr Right) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
    }
}
