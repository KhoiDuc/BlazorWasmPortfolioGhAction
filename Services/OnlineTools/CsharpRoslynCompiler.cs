using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorWasmPortfolioGhAction.Services.OnlineTools;

public sealed class CsharpCompilationMessage
{
    public DiagnosticSeverity Severity { get; init; }
    public string Message { get; init; } = "";
}

public sealed class CsharpCompilationResult
{
    public bool Success { get; init; }
    public IReadOnlyList<CsharpCompilationMessage> Messages { get; init; } = [];
    public byte[]? AssemblyBytes { get; init; }
}

public sealed class CsharpRoslynCompiler
{
    private readonly HttpClient _http;
    private Task? _initTask;
    private List<MetadataReference>? _references;

    public CsharpRoslynCompiler(HttpClient http) => _http = http;

    public Task EnsureInitializedAsync() => _initTask ??= InitializeAsync();

    private async Task InitializeAsync()
    {
        await using var stream = await _http.GetStreamAsync("_framework/blazor.boot.json");
        using var doc = await JsonDocument.ParseAsync(stream);
        var assemblyNames = ExtractAssemblyNames(doc.RootElement);

        var references = new List<MetadataReference>(assemblyNames.Count);
        foreach (var name in assemblyNames)
        {
            await using var dll = await _http.GetStreamAsync($"_framework/{name}");
            using var ms = new MemoryStream();
            await dll.CopyToAsync(ms);
            references.Add(MetadataReference.CreateFromImage(ms.ToArray()));
        }

        _references = references;
    }

    private static List<string> ExtractAssemblyNames(JsonElement root)
    {
        var names = new List<string>();
        if (!root.TryGetProperty("resources", out var resources))
            return names;

        // .NET 8 style: resources.assembly = { "Foo.dll": "sha..." }
        if (resources.TryGetProperty("assembly", out var assembly) && assembly.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in assembly.EnumerateObject())
                names.Add(prop.Name);
        }

        // Fingerprinted layout: resources.fingerprinting maps hash → logical name; assembly keys are hashes
        if (names.Count == 0 && resources.TryGetProperty("fingerprinting", out var fingerprinting))
        {
            foreach (var prop in fingerprinting.EnumerateObject())
            {
                if (prop.Value.GetString() is { } logical && logical.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    names.Add(prop.Name);
            }
        }

        return names;
    }

    public async Task<CsharpCompilationResult> CompileAsync(
        string source,
        LanguageVersion languageVersion = LanguageVersion.Latest,
        OptimizationLevel optimizationLevel = OptimizationLevel.Debug)
    {
        await EnsureInitializedAsync();

        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion));
        var compilation = CSharpCompilation.Create("DynamicCode")
            .WithOptions(new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                concurrentBuild: false,
                optimizationLevel: optimizationLevel,
                allowUnsafe: true))
            .AddReferences(_references!)
            .AddSyntaxTrees(syntaxTree);

        var diagnostics = compilation.GetDiagnostics();
        var messages = diagnostics
            .Select(d => new CsharpCompilationMessage { Severity = d.Severity, Message = d.ToString() })
            .ToList();

        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            return new CsharpCompilationResult { Success = false, Messages = messages };

        using var pe = new MemoryStream();
        var emit = compilation.Emit(pe);
        if (!emit.Success)
        {
            messages.AddRange(emit.Diagnostics.Select(d => new CsharpCompilationMessage
            {
                Severity = d.Severity,
                Message = d.ToString()
            }));
            return new CsharpCompilationResult { Success = false, Messages = messages };
        }

        return new CsharpCompilationResult
        {
            Success = true,
            Messages = messages,
            AssemblyBytes = pe.ToArray()
        };
    }

    public async Task<(bool ok, string output, IReadOnlyList<CsharpCompilationMessage> messages)> CompileAndRunAsync(
        string source,
        LanguageVersion languageVersion = LanguageVersion.Latest,
        OptimizationLevel optimizationLevel = OptimizationLevel.Debug)
    {
        var result = await CompileAsync(source, languageVersion, optimizationLevel);
        if (!result.Success || result.AssemblyBytes is null)
            return (false, "", result.Messages);

        var writer = new StringWriter();
        var previous = Console.Out;
        Console.SetOut(writer);
        try
        {
            var asm = Assembly.Load(result.AssemblyBytes);
            var entry = asm.EntryPoint
                ?? asm.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    .FirstOrDefault(m => m.Name is "Main" or "<Main>$");

            if (entry is null)
                return (false, writer.ToString(), result.Messages.Append(new CsharpCompilationMessage
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "No entry point (Main) found."
                }).ToList());

            var hasArgs = entry.GetParameters().Length == 1;
            var invokeResult = entry.Invoke(null, hasArgs ? [Array.Empty<string>()] : null);
            if (invokeResult is Task task)
                await task;

            return (true, writer.ToString(), result.Messages);
        }
        catch (Exception ex)
        {
            var msg = ex is TargetInvocationException { InnerException: { } inner } ? inner.ToString() : ex.ToString();
            return (false, writer.ToString() + msg, result.Messages);
        }
        finally
        {
            Console.SetOut(previous);
        }
    }
}
