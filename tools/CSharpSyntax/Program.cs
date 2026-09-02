using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class Program
{
    private static int Main(string[] args)
    {
        string root = args.Length == 1 ? args[0] : "Assets";
        int errors = 0, files = 0;
        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            files++;
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), new CSharpParseOptions(LanguageVersion.CSharp9), path: file);
            foreach (var diagnostic in tree.GetDiagnostics())
                if (diagnostic.Severity == DiagnosticSeverity.Error) { Console.Error.WriteLine(diagnostic); errors++; }
        }
        if (files == 0) { Console.Error.WriteLine("No C# sources found in " + root); return 1; }
        Console.WriteLine(files + " C# sources parsed; " + errors + " syntax errors. Unity API compilation still requires the editor.");
        return errors == 0 ? 0 : 1;
    }
}
