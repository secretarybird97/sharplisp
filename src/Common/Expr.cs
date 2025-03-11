using System.Text;

namespace SharpLisp.Common;

public abstract record Expr { }

public record IntLiteral(int Value) : Expr;
public record StringLiteral(string Value) : Expr;
public record DoubleLiteral(double Value) : Expr;

public record LetExpr(List<(string Identifier, Expr Value)> Bindings, Expr Body) : Expr;
public record BinaryExpr(string Operator, Expr Left, Expr Right) : Expr;
public record UnaryExpr(string Operator, Expr Operand) : Expr;

public record IdentifierExpr(string Name) : Expr;
public record CallExpr(Expr Callee, List<Expr> Args) : Expr;
public record FunctionDef(string Name, List<string> Parameters, Expr Body) : Expr;

public record IfExpr(Expr Condition, Expr ThenBranch, Expr ElseBranch) : Expr;
public record WhileExpr(Expr Condition, Expr Body) : Expr;
public record SetExpr(string Identifier, Expr Value) : Expr;
public record BlockExpr(List<Expr> Expressions) : Expr
{
    public override string ToString()
    {
        var st = new StringBuilder();
        for (int i = 0; i < Expressions.Count; i += 1)
        {
            st.AppendLine(Expressions[i].ToString());
        }

        return st.ToString();
    }
}

