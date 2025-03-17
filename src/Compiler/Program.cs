// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Diagnostics;

namespace SharpLisp.Compiler;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArg = new Argument<FileInfo?>(name: @"FILE", description: @"File to compile.") { Arity = ArgumentArity.ZeroOrOne };
        var codeOption = new Option<string?>(name: @"--code", description: @"Pass code directly instead of a file.");

        var rootCommand = new RootCommand(@"Execute a SharpLisp application.");
        rootCommand.AddArgument(fileArg);
        rootCommand.AddOption(codeOption);

        rootCommand.SetHandler(async (file, code) =>
        {
            if (file is not null)
            {
                RunCompiler(await ReadFile(file));
            }
            else if (code is not null)
            {
                RunCompiler(code);
            }
            // TODO: REPL
        }, fileArg, codeOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void RunCompiler(string input, bool printOut = true)
    {
        if (printOut)
        {
            var st = new Stopwatch();
            Console.WriteLine("Compiling . . .\n");
            st.Start();
            Compile(input);
            st.Stop();
            Console.WriteLine($"\nFinished! TimeElapsed: {st.ElapsedMilliseconds} ms");
        }
        else
        {
            Compile(input);
        }
    }

    static void Compile(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse();
#if DEBUG
        Console.WriteLine(ast + "\n");
#endif
        ILGeneratorBackend.Compile(ast);
    }

    static async Task<string> ReadFile(FileInfo file)
    {
        return await File.ReadAllTextAsync(file.FullName);
    }
}

