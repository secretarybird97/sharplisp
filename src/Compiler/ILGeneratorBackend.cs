namespace SharpLisp.Compiler;

using System;
using System.Reflection;
using System.Reflection.Emit;

public class ILGeneratorBackend
{
    public static void Compile(FunctionDef function)
    {
        var typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("LispOutput"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("MainModule")
            .DefineType("LispGenerated", TypeAttributes.Public);

        var methodBuilder = typeBuilder.DefineMethod(
            function.Name, MethodAttributes.Public | MethodAttributes.Static,
            typeof(double), [typeof(double)]);

        var il = methodBuilder.GetILGenerator();

        if (function.Body is BinaryExpr binary)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_R8, 23.2);
            il.Emit(binary.Operator switch
            {
                "*" => OpCodes.Mul,
                "+" => OpCodes.Add,
                "-" => OpCodes.Sub,
                "/" => OpCodes.Div,
                _ => throw new Exception("Unsupported operator")
            });
            il.Emit(OpCodes.Ret);
        }

        var generatedType = typeBuilder.CreateType();
        var generatedMethod = generatedType.GetMethod(function.Name);
        if (generatedMethod is not null)
        {
            var result = generatedMethod.Invoke(null, [5.2]);
            Console.WriteLine($"({function.Name}(5.2) = {result})");
        }
        else
        {
            Console.WriteLine("Method not found.");
        }
    }
}
