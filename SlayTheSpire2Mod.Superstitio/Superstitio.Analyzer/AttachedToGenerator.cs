using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Superstitio.Analyzer;

[Generator(LanguageNames.CSharp)]
public class AttachedToGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册一个初始化后立即执行的动作，把我们的特性注入进去
        context.RegisterPostInitializationOutput(ctx =>
        {
            // 资源的命名规则默认是：项目默认命名空间.文件夹名.文件名
            const string resourceName = "Superstitio.Analyzer.Injections.AttachedToAttribute.cs";
            
            var assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream != null)
            {
                using StreamReader reader = new StreamReader(stream);
                string source = reader.ReadToEnd();
                // 注入源码
                ctx.AddSource("AttachedToAttribute.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        });
    }
}