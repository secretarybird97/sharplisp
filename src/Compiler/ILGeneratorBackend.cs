namespace SharpLisp.Compiler;

using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpLisp.Common;

public static class ILGeneratorBackend
{
    private static readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder
        .DefineDynamicAssembly(new AssemblyName("LispOutput"), AssemblyBuilderAccess.RunAndCollect);
    private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder
        .DefineDynamicModule("LispOutput" + ".dll");

    private static readonly TypeBuilder TypeBuilder = ModuleBuilder.DefineType("Program", TypeAttributes.Public);

    private static readonly Dictionary<string, MethodBuilder> FunctionBuilders = [];
    public static void Compile(Expr expr)
    {
        var functions = ExtractFunctions(expr);

        // First pass: Define function signatures (without bodies)
        ref var ptrFunction = ref MemoryMarshal.GetReference(functions.AsSpan());
        for (int i = 0; i < functions.Length; i++)
        {
            var f = Unsafe.Add(ref ptrFunction, i);
            DefineFunctionSignature(f);
        }

        // Second pass: Generate function bodies
        for (int i = 0; i < functions.Length; i++)
        {
            var f = Unsafe.Add(ref ptrFunction, i);
            GenerateFunctionBody(f);
        }

        // ? Should function bodies be generated before or after main?

        // Third pass: Define Main
        _ = DefineMainMethod(expr);


        // Create type and invoke Main
        var generatedType = TypeBuilder.CreateType();
        var generatedMain = generatedType.GetMethod("Main");

        if (generatedMain is not null)
        {
            try
            {
                generatedMain.Invoke(null, null);
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine($"TargetInvocationException: {ex.Message}");
                if (ex.InnerException is not null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
        else
        {
            throw new Exception("Failed to locate generated Main method.");
        }
    }

    private static ImmutableArray<FunctionDef> ExtractFunctions(Expr expr)
    {
        var functions = ImmutableArray.CreateBuilder<FunctionDef>();

        if (expr is FunctionDef func)
        {
            functions.Add(func);
        }
        else if (expr is BlockExpr block)
        {
            ref var ptrExpr = ref MemoryMarshal.GetReference(block.Expressions.AsSpan());
            for (int i = 0; i < block.Expressions.Length; i += 1)
            {
                var subExpr = Unsafe.Add(ref ptrExpr, i);
                if (subExpr is FunctionDef f)
                {
                    functions.Add(f);
                }
            }
        }

        return functions.DrainToImmutable();
    }

    private static MethodBuilder DefineMainMethod(Expr expr)
    {
        var methodBuilder = TypeBuilder.DefineMethod(
            "Main", MethodAttributes.Public | MethodAttributes.Static,
            typeof(void), Type.EmptyTypes
        );

        var il = methodBuilder.GetILGenerator();

        // Generate code for the last expression
        GenerateExpressionIL(expr, il, []);

        // Print the result
        var writeLineMethod = typeof(Console).GetMethod("WriteLine", [typeof(object)]);
        if (writeLineMethod is not null)
        {
            il.Emit(OpCodes.Call, writeLineMethod);
        }
        else
        {
            throw new Exception("Failed to locate Console.WriteLine method.");
        }
        il.Emit(OpCodes.Ret);

        return methodBuilder;
    }

    private static void DefineFunctionSignature(FunctionDef function)
    {
        if (FunctionBuilders.ContainsKey(function.Name))
        {
            return;
        }

        var methodBuilder = TypeBuilder.DefineMethod(
            function.Name,
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(object), function.GetTypesArray()
        );
        // ? Should paramterTypes equal [typeof(object[])] or [typeof(object), typeof(object)...]

        FunctionBuilders[function.Name] = methodBuilder;
    }

    private static void GenerateFunctionBody(FunctionDef function)
    {
        if (!FunctionBuilders.TryGetValue(function.Name, out var methodBuilder))
        {
            throw new Exception($"Method {function.Name} not defined.");
        }

        var il = methodBuilder.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        // Store function arguments in locals
        ref var ptrParams = ref MemoryMarshal.GetReference(function.Parameters.AsSpan());
        for (int i = 0; i < function.Parameters.Length; i++)
        {
            var paramName = Unsafe.Add(ref ptrParams, i);
            var localVar = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Stloc, localVar);
            locals[paramName] = localVar;
        }

        // Generate the function body
        GenerateExpressionIL(function.Body, il, locals);
        il.Emit(OpCodes.Ret);
    }

    private static void GenerateExpressionIL(Expr expr, ILGenerator il, Dictionary<string, LocalBuilder> locals)
    {
        switch (expr)
        {
            case IntLiteral intLiteral:
                il.Emit(OpCodes.Ldc_I4, intLiteral.Value);
                il.Emit(OpCodes.Box, typeof(int));
                break;

            case StringLiteral stringLiteral:
                il.Emit(OpCodes.Ldstr, stringLiteral.Value);
                // ? Is boxing necessary with strings?
                break;

            case DoubleLiteral doubleLiteral:
                il.Emit(OpCodes.Ldc_R8, doubleLiteral.Value);
                il.Emit(OpCodes.Box, typeof(double));
                break;

            // TODO: Support dynamic types on function arguments. (only int currently)
            case BinaryExpr binaryExpr:
                GenerateExpressionIL(binaryExpr.Left, il, locals);
                il.Emit(OpCodes.Unbox_Any, typeof(int));

                GenerateExpressionIL(binaryExpr.Right, il, locals);
                il.Emit(OpCodes.Unbox_Any, typeof(int));

                il.Emit(binaryExpr.Operator switch
                {
                    "+" => OpCodes.Add,
                    "-" => OpCodes.Sub,
                    "*" => OpCodes.Mul,
                    "/" => OpCodes.Div,
                    _ => throw new Exception("Unsupported operator")
                });
                il.Emit(OpCodes.Box, typeof(int));
                break;

            // TODO
            case UnaryExpr unaryExpr:
                break;
            //     GenerateExpressionIL(unaryExpr.Operand, il, locals);
            //     il.Emit(unaryExpr.Operator switch
            //     {
            //         "-" => OpCodes.Neg,
            //         "!" => OpCodes.Not,
            //         _ => throw new Exception("Unsupported unary operator")
            //     });
            //     break;

            case IdentifierExpr identifierExpr:
                if (locals.TryGetValue(identifierExpr.Name, out var local))
                {
                    il.Emit(OpCodes.Ldloc, local);
                }
                else
                {
                    throw new Exception($"Variable {identifierExpr.Name} not defined.");
                }
                break;

            case CallExpr callExpr:
                if (!FunctionBuilders.TryGetValue(callExpr.Callee.Name, out var method))
                {
                    throw new Exception($"Method {callExpr.Callee.Name} not found.");
                }

                il.Emit(OpCodes.Ldc_I4, callExpr.Args.Length);
                il.Emit(OpCodes.Newarr, typeof(object));

                ref var ptrExpr = ref MemoryMarshal.GetReference(callExpr.Args.AsSpan());
                for (int i = 0; i < callExpr.Args.Length; i++)
                {
                    var subExpr = Unsafe.Add(ref ptrExpr, i);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    GenerateExpressionIL(subExpr, il, locals);
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Call, method);
                break;

            // TODO:
            case LetExpr letExpr:
                break;
            //     var letLocals = new Dictionary<string, LocalBuilder>(locals);

            //     foreach (var (Identifier, Value) in letExpr.Bindings)
            //     {
            //         GenerateExpressionIL(Value, il, letLocals);
            //         var localVar = il.DeclareLocal(typeof(object));
            //         il.Emit(OpCodes.Stloc, localVar);
            //         letLocals[Identifier] = localVar;
            //     }

            //     GenerateExpressionIL(letExpr.Body, il, letLocals);
            //     break;

            case BlockExpr blockExpr:
                ref var ptrExp = ref MemoryMarshal.GetReference(blockExpr.Expressions.AsSpan());
                for (int i = 0; i < blockExpr.Expressions.Length; i++)
                {
                    var exp = Unsafe.Add(ref ptrExp, i);
                    GenerateExpressionIL(exp, il, locals);
                }
                break;

            case FunctionDef:
                break;

            default:
                throw new Exception("Unsupported expression type");
        }
    }
}
