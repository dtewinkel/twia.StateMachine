using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twia.StateMachine.CodeGenerator.UnitTests.Verifiers;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class StateMachineIncrementalCodeGeneratorTests
{
    private readonly TestContext _testContext;

    private readonly IncrementalGeneratorVerifier<StateMachineIncrementalCodeGenerator> _verifier = new(typeof(StateMachineIncrementalCodeGeneratorTests));

    public StateMachineIncrementalCodeGeneratorTests(TestContext testContext)
    {
        _testContext = testContext;
        _verifier.AddAdditionalFileReferences("Twia.StateMachine.dll");
    }

    private static readonly string _initialStateTestDir = Path.Combine("TestFiles", "Sync");

    public static IEnumerable<object[]> InitialStateTestCases()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, _initialStateTestDir);
        if (!Directory.Exists(dir))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".e.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f))
        {
            var relative = Path.GetRelativePath(dir, file);
            yield return [relative];
        }
    }

    public static string InitialStateDisplayName(MethodInfo methodInfo, object[] data)
    {
        var relative = (string)data[0];
        var path = Path.Combine(AppContext.BaseDirectory, _initialStateTestDir, relative);
        var subDir = Path.GetDirectoryName(relative)?.Replace(Path.DirectorySeparatorChar, '/') ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(relative);
        if (File.Exists(path))
        {
            var header = ParseHeader(File.ReadAllText(path));
            if (!string.IsNullOrWhiteSpace(header.Name))
            {
                name = header.Name!;
            }
        }
        return string.IsNullOrEmpty(subDir) ? name : $"{subDir}: {name}";
    }

    [TestMethod]
    [DynamicData(
        nameof(InitialStateTestCases),
        DynamicDataDisplayName = nameof(InitialStateDisplayName))]
    public async Task Generator_FromSourceFile_GeneratesCorrectResult(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, _initialStateTestDir, fileName);
        var code = await File.ReadAllTextAsync(path, _testContext.CancellationToken);
        var header = ParseHeader(code);

        if (!header.HasHeader)
        {
            Assert.Fail($"Test file '{fileName}' is missing a header block (/*** ... ***/).");
        }

        var output = header.Output ?? "None";
        if (!string.Equals(output, "Source", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(output, "None", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail($"Test file '{fileName}' has invalid Output '{output}'. Expected 'Source' or 'None'.");
        }

        if (string.Equals(output, "Source", StringComparison.OrdinalIgnoreCase))
        {
            var expectedPath = Path.ChangeExtension(path, null) + ".e.cs";
            if (!File.Exists(expectedPath))
            {
                Assert.Fail($"Expected generated source file '{Path.GetFileName(expectedPath)}' is missing for test '{fileName}'.");
            }

            var expectedCode = await File.ReadAllTextAsync(expectedPath, _testContext.CancellationToken);
            if (header.Diagnostics.Count == 0)
            {
                await _verifier.VerifyGeneratorAsync([code], ("*UnitTestEmptyStateMachine*", expectedCode));
            }
            else
            {
                await _verifier.VerifyGeneratorAsyncWithDiagnostics([code], [.. header.Diagnostics], ("*UnitTestEmptyStateMachine*", expectedCode));
            }
            return;
        }

        if (header.Diagnostics.Count == 0)
        {
            await _verifier.VerifyGeneratorAsyncWithEmptyResult([code]);
        }
        else
        {
            await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [.. header.Diagnostics]);
        }
    }

    private sealed record HeaderInfo(bool HasHeader, string? Name, string? Output, List<DiagnosticResult> Diagnostics);

    private static readonly Regex _headerBlockRegex = new(
        @"/\*\*\*(?<body>.*?)\*\*\*/",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex _nameRegex = new(
        @"^\s*\*\s*Name\s*:\s*(?<name>.+?)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex _outputRegex = new(
        @"^\s*\*\s*Output\s*:\s*(?<output>.+?)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex _diagnosticLineRegex = new(
        @"^\s*\*\s*-\s*(?<id>SMG\d+)\s*,\s*(?<line>\d+)\s*,\s*(?<column>\d+)(?<args>(?:\s*,\s*""(?:[^""\\]|\\.)*"")*)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex _quotedArgRegex = new(
        @"""(?<value>(?:[^""\\]|\\.)*)""",
        RegexOptions.Compiled);

    private static HeaderInfo ParseHeader(string source)
    {
        var diagnostics = new List<DiagnosticResult>();
        string? name = null;

        var blockMatch = _headerBlockRegex.Match(source);
        if (!blockMatch.Success)
        {
            return new HeaderInfo(false, name, null, diagnostics);
        }

        var body = blockMatch.Groups["body"].Value;

        var nameMatch = _nameRegex.Match(body);
        if (nameMatch.Success)
        {
            name = nameMatch.Groups["name"].Value.Trim();
        }

        string? output = null;
        var outputMatch = _outputRegex.Match(body);
        if (outputMatch.Success)
        {
            output = outputMatch.Groups["output"].Value.Trim();
        }

        foreach (Match match in _diagnosticLineRegex.Matches(body))
        {
            var id = match.Groups["id"].Value;
            var line = int.Parse(match.Groups["line"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var column = int.Parse(match.Groups["column"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var args = _quotedArgRegex.Matches(match.Groups["args"].Value)
                .Select(m => Regex.Unescape(m.Groups["value"].Value))
                .Cast<object>()
                .ToArray();

            var result = DiagnosticResult
                .CompilerError(id)
                .WithLocation(line, column)
                .WithArguments(args);
            diagnostics.Add(result);
        }

        return new HeaderInfo(true, name, output, diagnostics);
    }
}
