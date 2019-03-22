﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeGeneration.CSharp;
using Roslynator.Metadata;

namespace Roslynator.CodeGeneration
{
    internal static class Program
    {
        private static readonly Regex _analyzerIdRegex = new Regex(@"^RCS\d+", RegexOptions.IgnoreCase);
        private static readonly Regex _refactoringIdRegex = new Regex(@"^RR\d+", RegexOptions.IgnoreCase);
        private static readonly Regex _codeFixIdRegex = new Regex(@"^(CS|VB)\d+", RegexOptions.IgnoreCase);

        private static void Main(string[] args)
        {
            string rootPath = args[0];

            var metadata = new RoslynatorMetadata(rootPath);

            ImmutableArray<AnalyzerDescriptor> analyzers = metadata.Analyzers;
            ImmutableArray<RefactoringDescriptor> refactorings = metadata.Refactorings;
            ImmutableArray<CompilerDiagnosticMetadata> compilerDiagnostics = metadata.CompilerDiagnostics;

            foreach (string id in args.Skip(1))
            {
                if (_analyzerIdRegex.IsMatch(id))
                {
                    AnalyzerDescriptor analyzer = analyzers.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase));

                    if (analyzer == null)
                    {
                        Console.WriteLine($"Analyzer '{id}' not found");
                        continue;
                    }

                    string className = $"{analyzer.Id}{analyzer.Identifier}Tests";

                    WriteCompilationUnit(
                        $@"Tests\Analyzers.Tests\{className}.cs",
                        AnalyzerTestGenerator.Generate(analyzer, className), autoGenerated: false, normalizeWhitespace: false, fileMustExist: false, overwrite: false);
                }
                else if (_refactoringIdRegex.IsMatch(id))
                {
                    RefactoringDescriptor refactoring = refactorings.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase));

                    if (refactoring == null)
                    {
                        Console.WriteLine($"Refactoring '{id}' not found");
                        continue;
                    }

                    string className = $"{refactoring.Id}{refactoring.Identifier}Tests";

                    WriteCompilationUnit(
                        $@"Tests\Refactorings.Tests\{className}.cs",
                        RefactoringTestGenerator.Generate(refactoring, className), autoGenerated: false, normalizeWhitespace: false, fileMustExist: false, overwrite: false);
                }
                else if (_codeFixIdRegex.IsMatch(id))
                {
                    CompilerDiagnosticMetadata compilerDiagnostic = compilerDiagnostics.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase));

                    if (compilerDiagnostic == null)
                    {
                        Console.WriteLine($"Compiler diagnostic '{id}' not found");
                        continue;
                    }

                    string className = $"{compilerDiagnostic.Id}{compilerDiagnostic.Identifier}Tests";

                    WriteCompilationUnit(
                        $@"Tests\CodeFixes.Tests\{className}.cs",
                        CodeFixTestGenerator.Generate(compilerDiagnostic, className), autoGenerated: false, normalizeWhitespace: false, fileMustExist: false, overwrite: false);
                }
                else
                {
                    Console.WriteLine($"Id '{id}' not recognized");
                }
            }

            void WriteCompilationUnit(
                string path,
                CompilationUnitSyntax compilationUnit,
                bool autoGenerated = true,
                bool normalizeWhitespace = true,
                bool fileMustExist = true,
                bool overwrite = true)
            {
                CodeGenerationHelpers.WriteCompilationUnit(
                    path: Path.Combine(rootPath, path),
                    compilationUnit: compilationUnit,
                    banner: CodeGenerationHelpers.CopyrightBanner,
                    autoGenerated: autoGenerated,
                    normalizeWhitespace: normalizeWhitespace,
                    fileMustExist: fileMustExist,
                    overwrite: overwrite);
            }
        }
    }
}
