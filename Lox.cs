namespace SharpLox;

public static class Lox
{
    private static bool HadError;

    public static void RunFile(string path)
    {
        var source = File.ReadAllText(path);
        Run(source);

        // Indicate an error in the exit code.
        if (HadError) Environment.Exit(65);
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

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var expression = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Console.WriteLine(new AstPrinter().Print(expression!));
    }

    private static void Report(int line, string where, string message)
    {
        var error = $"[line {line}] Error{where}: {message}";
        Console.Error.WriteLine(error);
        HadError = true;
    }
}
