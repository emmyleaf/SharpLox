using static SharpLox.TokenType;

namespace SharpLox;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new();

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private bool AtEnd => current >= source.Length;

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> ScanTokens()
    {
        while(!AtEnd)
        {
            // We are at the beginning of the next lexeme.
            start = current;
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
                if (Match('/')) {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !AtEnd) Advance();
                } else {
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

            // Everything else is an error
            default:
                Lox.Error(line, "Unexpected character.");
                break;
        }
    }

    private char Advance() {
        return source[current++];
    }

    private void AddToken(TokenType type) {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal) {
        var text = source.Substring(start, current);
        tokens.Add(new Token(type, text, literal, line));
    }

    private bool Match(char expected) {
        if (AtEnd) return false;
        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private char Peek() {
        if (AtEnd) return '\0';
        return source[current];
    }
}
