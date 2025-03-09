// See https://aka.ms/new-console-template for more information
using SharpLisp.Compiler;

Console.WriteLine("Hello, World!");

// var lexer = new Lexer("(defn square (x) (* x x))");
// Token token;
// while ((token = lexer.NextToken()).Type != TokenType.EOF)
//     Console.WriteLine($"{token.Type}: {token.Value}");
const string input = "(fn square (x) (* x x))";
var lexer = new Lexer(input);
// Console.WriteLine(lexer.GetTokens().Aggregate("", (acc, t) => acc + t.Value + "\n"));
var parser = new Parser(lexer);
var ast = parser.Parse();
Console.WriteLine(ast);
if (ast is FunctionDef functionDef)
{
    ILGeneratorBackend.Compile(functionDef);
}
else
{
    Console.WriteLine("Error: AST is not a FunctionDef.");
}
