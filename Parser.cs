using static SharpLox.TokenType;

namespace SharpLox;

public class Parser
{
    private class ParseError : Exception { }

    private readonly List<Token> tokens;
    private int currentPos = 0;

    private bool AtEnd => Current.Type == EOF;
    private Token Current => tokens[currentPos];
    private Token Previous => tokens[currentPos - 1];

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!AtEnd)
        {
            var decl = Declaration();
            if (decl is not null) statements.Add(decl);
        }

        return statements;
    }

    #region Rules

    private Stmt? Declaration()
    {
        try
        {
            if (Match(FUN)) return FunctionDeclaration("function");
            if (Match(VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt.Function FunctionDeclaration(string kind)
    {
        var name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = new();
        if (!Check(RIGHT_PAREN))
        {
            parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            while (Match(COMMA))
            {
                if (parameters.Count >= 255) Error(Current, "Can't have more than 255 parameters.");
                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            }
        }
        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
        var body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(FOR)) return ForStatement();
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(RETURN)) return ReturnStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");

        var initializer = Match(SEMICOLON) ? null :
            Match(VAR) ? VarDeclaration() : ExpressionStatement();

        var condition = Check(SEMICOLON) ? null : Expression();
        Consume(SEMICOLON, "Expect ';' after loop condition.");

        var increment = Check(RIGHT_PAREN) ? null : Expression();
        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

        var body = Statement();

        if (increment is not null)
        {
            var incStmt = new Stmt.Expression(increment);
            body = new Stmt.Block(new List<Stmt> { body, incStmt });
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer is not null)
        {
            body = new Stmt.Block(new List<Stmt> { initializer, body });
        }

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        var thenBranch = Statement();
        var elseBranch = Match(ELSE) ? Statement() : null;

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
        var keyword = Previous;
        var value = Check(SEMICOLON) ? null : Expression();

        Consume(SEMICOLON, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after while condition.");
        var body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new();

        while (!Check(RIGHT_BRACE) && !AtEnd)
        {
            var decl = Declaration();
            if (decl is not null) statements.Add(decl);
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = LogicalOr();

        if (Match(EQUAL))
        {
            var equals = Previous;
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                return new Expr.Assign(variable.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr LogicalOr()
    {
        var expr = LogicalAnd();

        while (Match(OR))
        {
            var op = Previous;
            var right = LogicalAnd();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr LogicalAnd()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var op = Previous;
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var op = Previous;
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var op = Previous;
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(MINUS, PLUS))
        {
            var op = Previous;
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(SLASH, STAR))
        {
            var op = Previous;
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            var op = Previous;
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous.Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous);
        }

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Current, "Expect expression.");
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Current, "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
    }

    #endregion

    #region Helpers

    private Token Advance()
    {
        if (!AtEnd) currentPos++;
        return Previous;
    }

    private bool Check(TokenType type)
    {
        if (AtEnd) return false;
        return Current.Type == type;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Current, message);
    }

    private ParseError Error(Token token, String message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private void Synchronize()
    {
        Advance();

        while (!AtEnd)
        {
            if (Previous.Type == SEMICOLON) return;

            switch (Current.Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    #endregion
}
