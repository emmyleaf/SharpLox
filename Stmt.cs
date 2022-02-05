namespace SharpLox;

public interface Stmt
{
    public interface Visitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
    }

    T Accept<T>(Visitor<T> visitor);

    public record Expression(Expr Expr) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitExpressionStmt(this);
    }

    public record Print(Expr Expr) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitPrintStmt(this);
    }
}