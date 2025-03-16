using System.Collections.Immutable;
using System.Diagnostics;
using SharpLisp.Common;

namespace SharpLisp.Compiler;

public sealed class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
    }

    private void Consume(TokenKind expected)
    {
        if (_currentToken.Kind == expected)
        {
            _currentToken = _lexer.NextToken();
        }
        else
        {
            throw new Exception($"Expected {expected}, got {_currentToken.Kind}");
        }
    }

    public Expr Parse()
    {
        if (_currentToken.Kind == TokenKind.EOF)
        {
            throw new Exception("Can't parse empty file");
        }

        var exprs = ImmutableArray.CreateBuilder<Expr>();
        while (_currentToken.Kind != TokenKind.EOF)
        {
            exprs.Add(ParseExpression());
        }

        Debug.Assert(exprs.Count > 0);
        return exprs.Count > 1 ? new BlockExpr(exprs.DrainToImmutable()) : exprs[0];
    }

    private Expr ParseExpression()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.LParen:
                Consume(TokenKind.LParen);
                return _currentToken.Kind switch
                {
                    TokenKind.Let => ParseLet(),
                    TokenKind.If => ParseIf(),
                    TokenKind.While => ParseWhile(),
                    TokenKind.Ident => ParseCall(),
                    TokenKind.Fn => ParseFunctionDef(),
                    TokenKind.Operator => ParseBinary(),
                    _ => throw new Exception("Unknown expression type: " + _currentToken.Kind),
                };
            case TokenKind.Number:
                var intExpr = new IntLiteral(int.Parse(_currentToken.Value));
                Consume(TokenKind.Number);
                return intExpr;
            case TokenKind.Ident:
                var identExpr = new IdentifierExpr(_currentToken.Value);
                Consume(TokenKind.Ident);
                return identExpr;
            default:
                throw new Exception("Unexpected token: " + _currentToken.Kind);
        }
    }

    private FunctionDef ParseFunctionDef()
    {
        Consume(TokenKind.Fn);
        string name = _currentToken.Value;
        Consume(TokenKind.Ident);
        Consume(TokenKind.LParen);

        var param = ImmutableArray.CreateBuilder<string>();
        while (_currentToken.Kind == TokenKind.Ident)
        {
            param.Add(_currentToken.Value);
            Consume(TokenKind.Ident);
        }
        Consume(TokenKind.RParen);
        var body = ParseExpression();
        Consume(TokenKind.RParen);

        return new FunctionDef(name, param.DrainToImmutable(), body);
    }

    private LetExpr ParseLet()
    {
        Consume(TokenKind.Let);
        Consume(TokenKind.LParen);

        var bindings = ImmutableArray.CreateBuilder<(string, Expr)>();
        while (_currentToken.Kind == TokenKind.Ident)
        {
            string name = _currentToken.Value;
            Consume(TokenKind.Ident);
            Expr value = ParseExpression();
            bindings.Add((name, value));
        }

        Consume(TokenKind.RParen);
        Expr body = ParseExpression();
        Consume(TokenKind.RParen);
        return new LetExpr(bindings.DrainToImmutable(), body);
    }

    private IfExpr ParseIf()
    {
        Consume(TokenKind.If);
        Expr condition = ParseExpression();
        Expr thenBranch = ParseExpression();
        Expr elseBranch = ParseExpression();
        Consume(TokenKind.RParen);
        return new IfExpr(condition, thenBranch, elseBranch);
    }

    private WhileExpr ParseWhile()
    {
        Consume(TokenKind.While);
        Expr condition = ParseExpression();
        Expr body = ParseExpression();
        Consume(TokenKind.RParen);
        return new WhileExpr(condition, body);
    }

    private CallExpr ParseCall()
    {
        var callee = new IdentifierExpr(_currentToken.Value);
        Consume(TokenKind.Ident);

        var args = ImmutableArray.CreateBuilder<Expr>();
        while (_currentToken.Kind != TokenKind.RParen)
        {
            args.Add(ParseExpression());
        }

        Consume(TokenKind.RParen);

        return new CallExpr(callee, args.DrainToImmutable());
    }

    private BinaryExpr ParseBinary()
    {
        string op = _currentToken.Value;
        Consume(TokenKind.Operator);
        Expr left = ParseExpression();
        Expr right = ParseExpression();
        Consume(TokenKind.RParen);
        return new BinaryExpr(op, left, right);
    }
}
