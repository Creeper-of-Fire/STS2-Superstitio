using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Superstitio.DesignConceptAnalyzer;

/// <inheritdoc />
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DesignConceptAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// 
    /// </summary>
    public const string DiagnosticId = "ST001";

    /// <summary>
    /// 
    /// </summary>
    public const string ConfigErrorId = "ST000";

    private const string ConfigFileName = "concepts.toml";

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        new(
            DiagnosticId,
            "设计原型",
            "[原型：{0}]",
            "Design",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            customTags: [WellKnownDiagnosticTags.Unnecessary]
        ),
        new(ConfigErrorId,
            "配置加载失败",
            "Analyzer加载配置时出错: {0}",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        )
    ];


    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // 使用 CompilationStartAction 来读取配置文件
        // 这样文件只会在编译开始或文件改变时读取一次，性能更好
        context.RegisterCompilationStartAction(compilationContext =>
        {
            try
            {
                // 1. 从 AdditionalFiles 中寻找指定的文件
                var configFile = compilationContext.Options.AdditionalFiles
                    .FirstOrDefault(f => Path.GetFileName(f.Path).Equals(ConfigFileName, StringComparison.OrdinalIgnoreCase));


                if (configFile is null)
                {
                    // 如果找不到文件，报一个警告
                    var diag = Diagnostic.Create(this.SupportedDiagnostics[1], Location.None, "未找到 concepts.toml 附加文件");
                    compilationContext.RegisterSymbolAction(c => c.ReportDiagnostic(diag), SymbolKind.NamedType);
                    return;
                }

                // 2. 读取文件内容
                var sourceText = configFile.GetText(compilationContext.CancellationToken);
                if (sourceText is null)
                {
                    // 如果找不到文件，报一个警告
                    var diag = Diagnostic.Create(this.SupportedDiagnostics[1], Location.None, "未找到 concepts.toml 文件内容");
                    compilationContext.RegisterSymbolAction(c => c.ReportDiagnostic(diag), SymbolKind.NamedType);
                    return;
                }

                // 3. 解析配置
                var mappings = ParseSimpleToml(sourceText.ToString());

                if (mappings.Count == 0)
                {
                    // 如果找不到文件，报一个警告
                    var diag = Diagnostic.Create(this.SupportedDiagnostics[1], Location.None, "未解析 concepts.toml 成功或为空");
                    compilationContext.RegisterSymbolAction(c => c.ReportDiagnostic(diag), SymbolKind.NamedType);
                    return;
                }

                // 4. 注册符号分析，并将解析出的 mappings 传进去
                compilationContext.RegisterSymbolAction(symbolContext => { this.AnalyzeSymbol(symbolContext, mappings); },
                    SymbolKind.NamedType);
            }
            catch (Exception ex)
            {
                // 捕获所有未知异常（如读取文件失败等）
                compilationContext.RegisterCompilationEndAction(endContext =>
                {
                    var diag = Diagnostic.Create(this.SupportedDiagnostics[1], Location.None, "全局异常: " + ex);
                    endContext.ReportDiagnostic(diag);
                });
            }
        });
    }

    /// <summary>
    /// 简易 TOML 解析：支持 Key = "Value" 和 # 注释
    /// </summary>
    private static Dictionary<string, string> ParseSimpleToml(string content)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // 跳过空行和注释
            if (string.IsNullOrEmpty(trimmedLine))
                continue;
            if (trimmedLine.StartsWith("#", StringComparison.Ordinal) || trimmedLine.StartsWith(";", StringComparison.Ordinal))
                continue;

            // 处理 Key = Value
            int separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0) continue;

            string key = trimmedLine.Substring(0, separatorIndex).Trim();
            string value = trimmedLine.Substring(separatorIndex + 1).Trim();

            // 去掉 Value 两边的引号 (如果有)
            if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal) && value.Length >= 2)
            {
                value = value.Substring(1, value.Length - 2);
            }

            if (!string.IsNullOrEmpty(key))
            {
                dict[key] = value;
            }
        }

        return dict;
    }


    private void AnalyzeSymbol(SymbolAnalysisContext context, Dictionary<string, string> mappings)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!mappings.TryGetValue(namedTypeSymbol.Name, out string? originalConcept))
            return;

        var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference is null)
            return;

        var syntaxNode = syntaxReference.GetSyntax(context.CancellationToken);

        Location nameLocation;
        if (syntaxNode is BaseTypeDeclarationSyntax typeDeclaration)
        {
            // 只高亮类名/接口名本身
            nameLocation = typeDeclaration.Identifier.GetLocation();
        }
        else
        {
            nameLocation = namedTypeSymbol.Locations[0];
        }

        var diagnostic = Diagnostic.Create(
            this.SupportedDiagnostics[0],
            nameLocation,
            originalConcept);

        context.ReportDiagnostic(diagnostic);
    }
}