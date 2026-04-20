using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Superstitio.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttachedToAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeFullName = "Superstitio.Analyzer.AttachedToAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        id: "AT001",
        title: "接口实现者受限",
        messageFormat: "类型 '{0}' 不允许实现接口 '{1}'。该接口只能由 {2} 实现",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        // 只关心 class / struct
        if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct)
            return;

        foreach (var iface in type.AllInterfaces)
        {
            var allowedTypes = GetAttachedToTypes(iface);
            if (allowedTypes.IsEmpty) continue;

            // 检查当前类型或其任意基类是否在允许列表中
            if (IsTypeOrBaseTypeAllowed(type, allowedTypes))
                continue;

            string allowedNames = string.Join(", ", allowedTypes.Select(t => t.Name));
            var diagnostic = Diagnostic.Create(Rule,
                type.Locations.FirstOrDefault(),
                type.Name, iface.Name, allowedNames);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsTypeOrBaseTypeAllowed(INamedTypeSymbol type, ImmutableArray<INamedTypeSymbol> allowedTypes)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (Enumerable.Any(allowedTypes, allowed => SymbolEqualityComparer.Default.Equals(current, allowed)))
            {
                return true;
            }
        }

        return false;
    }

    private static ImmutableArray<INamedTypeSymbol> GetAttachedToTypes(INamedTypeSymbol iface)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (var attr in iface.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != AttributeFullName) 
                continue;
            
            if (attr.ConstructorArguments.Length == 0) 
                continue;

            if (attr.ConstructorArguments[0].Value is INamedTypeSymbol namedTargetType)
            {
                builder.Add(namedTargetType);
            }
        }

        return builder.ToImmutable();
    }
}