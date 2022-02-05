namespace SharpLox;

public static class Lox
{
    private static readonly Interpreter Interpreter = new();
    private static bool HadError;
    private static bool HadRuntimeError;

    public static void RunFile(string path)
    {
        var source = File.ReadAllText(path);
        Run(source);

        // Indicate an error in the exit code.
        if (HadError) Environment.Exit(65);
        if (HadRuntimeError) Environment.Exit(70);
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            HadError = false;
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, String message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, $" at '{token.Lexeme}'", message);
        }
    }

    public static void RuntimeError(RuntimeError error)
    {
        var errMessage = $"{error.Message}\n[line {error.Token.Line}]";
        Console.Error.WriteLine(errMessage);
        HadRuntimeError = true;
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Interpreter.Interpret(statements);
    }

    private static void Report(int line, string where, string message)
    {
        var errMessage = $"[line {line}] Error{where}: {message}";
        Console.Error.WriteLine(errMessage);
        HadError = true;
    }
}
