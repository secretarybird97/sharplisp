namespace SharpLisp.Compiler;

public enum TokenKind : byte
{
    Fn,
    Let,
    LSBracket,
    RSBracket,
    LBracket,
    RBracket,
    Ident,
    Number,
    LParen,
    RParen,
    Operator,
    EOF,
    Backslash,
    Var,
    Async,
    Await,
    Class,
    If,
    While
}
