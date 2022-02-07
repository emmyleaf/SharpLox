namespace SharpLox;

public interface Stmt
{
    public interface Visitor<T>
    {
        T VisitBlockStmt(Block stmt);
        T VisitExpressionStmt(Expression stmt);
        T VisitIfStmt(If stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitWhileStmt(While stmt);
    }

    T Accept<T>(Visitor<T> visitor);

    public record Block(List<Stmt> statements) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitBlockStmt(this);
    }

    public record Expression(Expr Expr) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitExpressionStmt(this);
    }

    public record If(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitIfStmt(this);
    }

    public record Print(Expr Expr) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitPrintStmt(this);
    }

    public record Var(Token Name, Expr? Initializer) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitVarStmt(this);
    }

    public record While(Expr Condition, Stmt Body) : Stmt
    {
        public T Accept<T>(Visitor<T> visitor) => visitor.VisitWhileStmt(this);
    }
}
