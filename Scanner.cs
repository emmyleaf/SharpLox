using static SharpLox.TokenType;

namespace SharpLox;

public class Scanner
{
    private static readonly IReadOnlyDictionary<string, TokenType> Keywords = new Dictionary<string, TokenType> {
        { "and",    AND },
        { "class",  CLASS },
        { "else",   ELSE },
        { "false",  FALSE },
        { "for",    FOR },
        { "fun",    FUN },
        { "if",     IF },
        { "nil",    NIL },
        { "or",     OR },
        { "print",  PRINT },
        { "return", RETURN },
        { "super",  SUPER },
        { "this",   THIS },
        { "true",   TRUE },
        { "var",    VAR },
        { "while",  WHILE }
    };

    private readonly string source;
    private readonly List<Token> tokens = new();

    private int startPos = 0;
    private int currentPos = 0;
    private int line = 1;

    private bool AtEnd => currentPos >= source.Length;
    private char Current => AtEnd ? '\0' : source[currentPos];
    private char Next => (currentPos + 1 >= source.Length) ? '\0' : source[currentPos + 1];
    private string TokenText => source[startPos..currentPos];

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!AtEnd)
        {
            // We are at the beginning of the next lexeme.
            startPos = currentPos;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));
        return tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            // Always single char tokens
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break;

            // Single or double char tokens
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;

            // Comment or single slash
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Current != '\n' && !AtEnd) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;

            // Ignore whitespace
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;

            // Strings
            case '"': ScanString(); break;

            // Scan for alphanumerics, otherwise error
            default:
                if (IsDigit(c))
                {
                    ScanNumber();
                }
                else if (IsAlpha(c))
                {
                    ScanIdentifier();
                }
                else
                {
                    Lox.Error(line, "Unexpected character.");
                }
                break;
        }
    }

    private char Advance() => source[currentPos++];

    private void AddToken(TokenType type, object? literal = null)
    {
        tokens.Add(new Token(type, TokenText, literal, line));
    }

    private bool Match(char expected)
    {
        if (AtEnd) return false;
        if (source[currentPos] != expected) return false;

        currentPos++;
        return true;
    }

    private void ScanIdentifier()
    {
        while (IsAlphaNumeric(Current)) Advance();

        AddToken(Keywords.GetValueOrDefault(TokenText, IDENTIFIER));
    }

    private void ScanNumber()
    {
        while (IsDigit(Current)) Advance();

        // Look for a fractional part.
        if (Current == '.' && IsDigit(Next))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Current)) Advance();
        }

        var value = double.Parse(TokenText);
        AddToken(NUMBER, value);
    }

    private void ScanString()
    {
        while (Current != '"' && !AtEnd)
        {
            if (Current == '\n') line++;
            Advance();
        }

        if (AtEnd)
        {
            Lox.Error(line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        var value = source[(startPos + 1)..(currentPos - 1)];
        AddToken(STRING, value);
    }

    private static bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    private static bool IsDigit(char c) => c >= '0' && c <= '9';
}
