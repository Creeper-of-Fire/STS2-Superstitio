using System.Collections.Generic;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.NameRelink;

// 注册分析器：目标是 C# 的类型声明（类、接口等）
[ElementProblemAnalyzer(typeof(ITypeDeclaration), HighlightingTypes = [typeof(DesignConceptHighlighting)])]
public class DesignConceptAnalyzer : ElementProblemAnalyzer<ITypeDeclaration>
{
    private static readonly Dictionary<string, string> Mappings = new()
    {
        { "PrepareToFight", "Masturbate" }
    };

    protected override void Run(ITypeDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
    {
        // element.DeclaredName 就是类名
        if (!Mappings.TryGetValue(element.DeclaredName, out string concept))
            return;

        // 获取类名标识符的位置（比如 "PrepareToFight" 这几个字符的范围）
        DocumentRange nameRange = element.GetNameDocumentRange();

        // 将高亮提示推送到界面上
        consumer.AddHighlighting(new DesignConceptHighlighting(nameRange, concept));
    }
}