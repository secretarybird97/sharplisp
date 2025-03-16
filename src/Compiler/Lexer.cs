using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SharpLisp.Common;

namespace SharpLisp.Compiler;

public sealed class Lexer(string input)
{
    private readonly string _input = input;
    private int _position = 0;

    public static readonly ReadOnlyDictionary<string, TokenKind> Keywords = new(new Dictionary<string, TokenKind>
    {
        {"fn", TokenKind.Fn},

        // TODO
        { "let", TokenKind.Let},
        {"var", TokenKind.Var},
        {"async", TokenKind.Async},
        {"await", TokenKind.Await},
        {"class", TokenKind.Class},
    });

    public static readonly ReadOnlyDictionary<char, TokenKind> Symbols = new(new Dictionary<char, TokenKind>
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
    });

    public Token NextToken()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position += 1;
        }

        if (_position >= _input.Length) return Token.EOF;

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
        int prevPosition = _position;

        _position = 0;
        var token = NextToken();
        while (token.Kind != TokenKind.EOF)
        {
            sb.AppendLine(token.ToString());
            token = NextToken();
        }

        _position = prevPosition;
        return sb.ToString();
    }

    public static IEnumerable<Token> Tokenize(string input)
    {
        var lexer = new Lexer(input);

        Token token;
        while ((token = lexer.NextToken()) != Token.EOF)
        {
            yield return token;
        }

        yield return token;
    }
}
