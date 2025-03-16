// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using SharpLisp.Compiler;

const string input = "(fn square (x) (* x x)) (square 5)";
var st = new Stopwatch();

Console.WriteLine("Compiling . . .\n");
st.Start();

var lexer = new Lexer(input);
var parser = new Parser(lexer);
var ast = parser.Parse();
Console.WriteLine(ast);

ILGeneratorBackend.Compile(ast);
st.Stop();

Console.WriteLine($"\nFinished! TimeElapsed: {st.ElapsedMilliseconds} ms");
