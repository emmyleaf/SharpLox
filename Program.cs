using SharpLox;

if (args.Length > 1)
{
    Console.WriteLine("Usage: SharpLox [script]");
    Environment.Exit(64);
}
else if (args.Length == 1)
{
    Lox.RunFile(args[0]);
}
else
{
    Lox.RunPrompt();
}
