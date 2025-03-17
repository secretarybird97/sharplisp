using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpLisp.Common;

public abstract record Expr { }

public sealed record IntLiteral(int Value) : Expr;
public sealed record StringLiteral(string Value) : Expr;
public sealed record DoubleLiteral(double Value) : Expr;

public sealed record LetExpr(ImmutableArray<(string Identifier, Expr Value)> Bindings, Expr Body) : Expr;
public sealed record BinaryExpr(string Operator, Expr Left, Expr Right) : Expr;
public sealed record UnaryExpr(string Operator, Expr Operand) : Expr;

public sealed record IdentifierExpr(string Name) : Expr;
public sealed record CallExpr(IdentifierExpr Callee, ImmutableArray<Expr> Args) : Expr;
public sealed record FunctionDef(string Name, ImmutableArray<string> Parameters, Expr Body) : Expr
{
    public Type[] GetTypesArray()
    {
        var arr = new Type[Parameters.Length];
        for (int i = 0; i < Parameters.Length; i += 1)
        {
            arr[i] = typeof(object);
        }
        return arr;
    }
}

public sealed record IfExpr(Expr Condition, Expr ThenBranch, Expr ElseBranch) : Expr;
public sealed record WhileExpr(Expr Condition, Expr Body) : Expr;
public sealed record SetExpr(string Identifier, Expr Value) : Expr;
public sealed record BlockExpr(ImmutableArray<Expr> Expressions) : Expr
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        ref var ptrExpr = ref MemoryMarshal.GetReference(Expressions.AsSpan());

        sb.AppendLine(@"BlockExpr {");
        for (int i = 0; i < Expressions.Length; i += 1)
        {
            var item = Unsafe.Add(ref ptrExpr, i);
            sb.AppendLine(@"  " + item.ToString());
        }
        sb.Append('}');

        return sb.ToString();
    }
}

