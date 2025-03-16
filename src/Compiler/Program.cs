// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Diagnostics;

namespace SharpLisp.Compiler;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArg = new Argument<FileInfo>(name: "FILE", description: "File to compile.");

        var rootCommand = new RootCommand("Execute a SharpLisp application.");
        rootCommand.AddArgument(fileArg);

        rootCommand.SetHandler(Run, fileArg);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task Run(FileInfo file)
    {
        Compile(await ReadFile(file));
    }

    static void Compile(string input)
    {
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
    }


    static async Task<string> ReadFile(FileInfo file)
    {
        return await File.ReadAllTextAsync(file.FullName);
    }
}

