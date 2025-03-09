using System.Runtime.InteropServices;

namespace SharpLisp.Compiler;

public class Parser
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
            _currentToken = _lexer.NextToken();
        else
            throw new Exception($"Expected {expected}, got {_currentToken.Kind}");
    }

    public IExpr Parse()
    {
        if (_currentToken.Kind == TokenKind.LParen)
        {
            Consume(TokenKind.LParen);
            return ParseExpression();
        }
        if (_currentToken.Kind == TokenKind.Number)
        {
            var expr = new IntLiteral(int.Parse(_currentToken.Value));
            Consume(TokenKind.Number);
            return expr;
        }
        if (_currentToken.Kind == TokenKind.Ident)
        {
            var expr = new IdentifierExpr(_currentToken.Value);
            Consume(TokenKind.Ident);
            return expr;
        }
        throw new Exception("Unexpected token: " + _currentToken.Kind);
    }

    private IExpr ParseExpression()
    {
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
    }

    private FunctionDef ParseFunctionDef()
    {
        Consume(TokenKind.Fn);
        string name = _currentToken.Value;
        Consume(TokenKind.Ident);
        Consume(TokenKind.LParen);

        List<string> param = [];
        while (_currentToken.Kind == TokenKind.Ident)
        {
            param.Add(_currentToken.Value);
            Consume(TokenKind.Ident);
        }
        Consume(TokenKind.RParen);
        var body = Parse();

        return new FunctionDef(name, param, body);
    }

    private LetExpr ParseLet()
    {
        Consume(TokenKind.Let);
        Consume(TokenKind.LParen);

        List<(string, IExpr)> bindings = [];
        while (_currentToken.Kind == TokenKind.Ident)
        {
            string name = _currentToken.Value;
            Consume(TokenKind.Ident);
            IExpr value = Parse();
            bindings.Add((name, value));
        }

        Consume(TokenKind.RParen);
        IExpr body = Parse();
        Consume(TokenKind.RParen);
        return new LetExpr(bindings, body);
    }

    private IfExpr ParseIf()
    {
        Consume(TokenKind.If);
        IExpr condition = Parse();
        IExpr thenBranch = Parse();
        IExpr elseBranch = Parse();
        Consume(TokenKind.RParen);
        return new IfExpr(condition, thenBranch, elseBranch);
    }

    private WhileExpr ParseWhile()
    {
        Consume(TokenKind.While);
        IExpr condition = Parse();
        IExpr body = Parse();
        Consume(TokenKind.RParen);
        return new WhileExpr(condition, body);
    }

    private CallExpr ParseCall()
    {
        var callee = new IdentifierExpr(_currentToken.Value);
        Consume(TokenKind.Ident);

        List<IExpr> args = [];
        while (_currentToken.Kind != TokenKind.RParen)
        {
            args.Add(Parse());
        }

        Consume(TokenKind.RParen);
        return new CallExpr(callee, args);
    }

    private BinaryExpr ParseBinary()
    {
        string op = _currentToken.Value;
        Consume(TokenKind.Operator);
        IExpr left = Parse();
        IExpr right = Parse();
        Consume(TokenKind.RParen);
        return new BinaryExpr(op, left, right);
    }
}
