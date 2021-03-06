namespace SharpLox;

public interface Expr
{
    public interface Visitor<T>
    {
        T VisitAssignExpr(Assign expr);
        T VisitBinaryExpr(Binary expr);
        T VisitCallExpr(Call expr);
        T VisitGetExpr(Get expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitLogicalExpr(Logical expr);
        T VisitSetExpr(Set expr);
        T VisitThisExpr(This expr);
        T VisitSuperExpr(Super expr);
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

    public record Call(Expr Callee, Token Paren, List<Expr> Arguments) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitCallExpr(this);
    }

    public record Get(Expr Object, Token Name) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitGetExpr(this);
    }

    public record Grouping(Expr Expression) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
    }

    public record Literal(object? Value) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
    }

    public record Logical(Expr Left, Token Operator, Expr Right) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitLogicalExpr(this);
    }

    public record Set(Expr Object, Token Name, Expr Value) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitSetExpr(this);
    }

    public record This(Token Keyword) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitThisExpr(this);
    }

    public record Super(Token Keyword, Token Method) : Expr
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitSuperExpr(this);
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
