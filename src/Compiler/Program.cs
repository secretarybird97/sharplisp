// See https://aka.ms/new-console-template for more information
using SharpLisp.Common;
using SharpLisp.Compiler;

const string input = "(fn square (x) (* x x))";

var lexer = new Lexer(input);
Console.WriteLine(lexer.TokensToString());

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
