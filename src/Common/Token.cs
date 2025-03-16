namespace SharpLisp.Common;

public readonly record struct Token(TokenKind Kind, string Value)
{
    public static Token EOF { get; } = new(TokenKind.EOF, "\0");
}
