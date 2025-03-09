namespace SharpLisp.Compiler;

public interface IExpr { }

public record IntLiteral(int Value) : IExpr;
public record StringLiteral(string Value) : IExpr;
public record DoubleLiteral(double Value) : IExpr;

public record LetExpr(List<(string Identifier, IExpr Value)> Bindings, IExpr Body) : IExpr;
public record BinaryExpr(string Operator, IExpr Left, IExpr Right) : IExpr;
public record UnaryExpr(string Operator, IExpr Operand) : IExpr;

public record IdentifierExpr(string Name) : IExpr;
public record CallExpr(IExpr Callee, List<IExpr> Args) : IExpr;
public record FunctionDef(string Name, List<string> Parameters, IExpr Body) : IExpr;

public record IfExpr(IExpr Condition, IExpr ThenBranch, IExpr ElseBranch) : IExpr;
public record WhileExpr(IExpr Condition, IExpr Body) : IExpr;
public record SetExpr(string Identifier, IExpr Value) : IExpr;
public record BlockExpr(List<IExpr> Expressions) : IExpr;

