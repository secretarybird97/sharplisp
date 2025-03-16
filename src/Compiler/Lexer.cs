using System.Text;
using SharpLisp.Common;

namespace SharpLisp.Compiler;

public sealed class Lexer(string input)
{
    private readonly string _input = input;
    private int _position = 0;

    private static readonly Dictionary<string, TokenKind> Keywords = new()
    {
        {"fn", TokenKind.Fn},

        // TODO
        { "let", TokenKind.Let},
        {"var", TokenKind.Var},
        {"async", TokenKind.Async},
        {"await", TokenKind.Await},
        {"class", TokenKind.Class},
    };

    private static readonly Dictionary<char, TokenKind> Symbols = new()
    {
        {'(', TokenKind.LParen},
        {')', TokenKind.RParen},
        {'+', TokenKind.Operator},
        {'-', TokenKind.Operator},
        {'*', TokenKind.Operator},
        {'/', TokenKind.Operator},

        // TODO
        {'[', TokenKind.LSBracket},
        {']', TokenKind.RSBracket},
        {'{', TokenKind.LBracket},
        {'}', TokenKind.RBracket},
        {'\\', TokenKind.Backslash}
    };

    public Token NextToken()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position += 1;
        }

        if (_position >= _input.Length) return new Token(TokenKind.EOF, "\0");

        char current = _input[_position];

        if (Symbols.TryGetValue(current, out TokenKind kind))
        {
            _position += 1;
            return new Token(kind, current.ToString());
        }

        if (char.IsDigit(current))
        {
            var num = ReadWhile(char.IsDigit);
            return new Token(TokenKind.Number, num);
        }

        if (char.IsLetter(current))
        {
            var ident = ReadWhile(char.IsLetterOrDigit);
            return new Token(Keywords.TryGetValue(ident, out TokenKind value) ? value : TokenKind.Ident, ident);
        }

        throw new Exception($"Unexpected character: {current}");
    }

    private string ReadWhile(Predicate<char> condition)
    {
        int start = _position;
        while (_position < _input.Length && condition(_input[_position]))
        {
            _position += 1;
        }
        return _input[start.._position];
    }

    public string TokensToString()
    {
        var sb = new StringBuilder();

        var token = NextToken();
        while (token.Kind != TokenKind.EOF)
        {
            sb.AppendLine(token.ToString());
            token = NextToken();
        }

        _position = 0;
        return sb.ToString();
    }
}
