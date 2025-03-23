using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Digillect.Sunstone.Resources.SourceGenerator.AttributeBased.Semantic;

namespace Digillect.Sunstone.Resources.SourceGenerator.AttributeBased;

[Generator]
public class AttributeBasedSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static ctx => {
			ctx.AddSource("KubernetesResourceAttribute.cs", SourceText.From(Constants.KubernetesResourceAttributeCode, Encoding.UTF8));
			ctx.AddSource("KubernetesPropertyAttribute.cs", SourceText.From(Constants.KubernetesPropertyAttributeCode, Encoding.UTF8));
		});

		var provider = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				Constants.KubernetesResourceAttributeTypeName,
				static (node, _) => node is ClassDeclarationSyntax,
				ResourceClassInspector.Inspect)
			.Where(static rc => rc is ResourceClass);

		context.RegisterSourceOutput(provider.Collect(), GenerateCode);
	}

	private static void GenerateCode(SourceProductionContext context, ImmutableArray<IResourceClass> resourceClasses)
	{
		var rcs = resourceClasses.OfType<ResourceClass>().ToImmutableArray();

		if (rcs.Length == 0)
		{
			return;
		}

		foreach (var resourceClass in rcs)
		{
			string code = ResourceCodeEmitter.Emit(resourceClass);

			context.AddSource($"{resourceClass.Namespace}.{resourceClass.Name}.g.cs", SourceText.From(code, Encoding.UTF8));
		}

		string? resourceFactoriesCode = ResourceFactoriesCodeEmitter.Emit(rcs);
		if (!string.IsNullOrEmpty(resourceFactoriesCode))
		{
			context.AddSource("ResourcesCollectionExtensions.g.cs", SourceText.From(resourceFactoriesCode!, Encoding.UTF8));
		}
	}
}
