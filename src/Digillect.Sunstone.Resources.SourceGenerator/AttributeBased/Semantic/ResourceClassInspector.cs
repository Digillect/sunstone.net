using System.Collections.Immutable;
using CaseExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Digillect.Sunstone.Resources.SourceGenerator.AttributeBased.Semantic;

public class ResourceClassInspector(SemanticModel semanticModel, INamedTypeSymbol classSymbol, ImmutableArray<AttributeData> attributes)
{
	private readonly ITypeSymbol _resourceAttributeType = semanticModel.Compilation.GetTypeByMetadataName(Constants.KubernetesResourceAttributeTypeName)!;
	private readonly ITypeSymbol _propertyAttributeType = semanticModel.Compilation.GetTypeByMetadataName(Constants.KubernetesPropertyAttributeTypeName)!;
	private readonly ITypeSymbol _dictionaryInterfaceType = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2")!;
	private readonly ITypeSymbol _baseArrayType = semanticModel.Compilation.GetTypeByMetadataName("Digillect.Sunstone.BaseArray`1")!;
	private readonly ITypeSymbol _baseObjectType = semanticModel.Compilation.GetTypeByMetadataName("Digillect.Sunstone.BaseObject")!;

	public static IResourceClass Inspect(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken)
	{
		var semanticModel = ctx.SemanticModel;
		var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.TargetNode;
		var typeSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax, cancellationToken);

		if (typeSymbol is not INamedTypeSymbol classSymbol)
		{
			return RejectedClass.Instance;
		}

		if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) || classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			return RejectedClass.Instance;
		}

		var inspector = new ResourceClassInspector(semanticModel, classSymbol, classSymbol.GetAttributes());

		return inspector.Inspect();
	}

	private IResourceClass Inspect()
	{
		(string? apiVersion, string? kind) = GetApiVersionAndKind();

		return new ResourceClass {
			Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
			Name = classSymbol.Name,

			ApiVersion = apiVersion!,
			Kind = kind!,

			Properties = GetProperties(),

			GenerateCreateMethod = !IsMethodDeclared("Create"),
			GeneratePersistMethod = !IsMethodDeclared("Persist")
		};
	}

	private IReadOnlyList<ResourceProperty> GetProperties()
	{
		return classSymbol
			.GetMembers()
			.OfType<IPropertySymbol>()
			.Where(p => !p.IsStatic)
			.Select(InspectProperty)
			.Where(p => p is not null)
			.ToImmutableList()!;
	}

	private ResourceProperty? InspectProperty(IPropertySymbol propertySymbol)
	{
		var attr = propertySymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _propertyAttributeType));

		var isIgnoredArgument = attr?.NamedArguments.FirstOrDefault(a => a.Key == "Ignore");
		if (isIgnoredArgument?.Value.Value is true)
		{
			return null;
		}

		if (propertySymbol.DeclaredAccessibility != Accessibility.Public || propertySymbol.IsStatic || propertySymbol.Type is not INamedTypeSymbol propertyTypeSymbol)
		{
			return null;
		}

		string name = propertySymbol.Name;
		string kubernetesName = name.ToCamelCase();
		KeyValuePair<string, TypedConstant>? nameArgument = attr?.NamedArguments.FirstOrDefault(a => a.Key == "Name");
		if (nameArgument?.Value.Value is string nameValue)
		{
			kubernetesName = nameValue;
		}

		int? persistencePriority = null;
		var persistencePriorityArgument = attr?.NamedArguments.FirstOrDefault(a => a.Key == "PersistencePriority");
		if (persistencePriorityArgument?.Value.Value is int persistencePriorityValue)
		{
			persistencePriority = persistencePriorityValue;
		}

		bool isPersisted = true;
		var isPersistedArgument = attr?.NamedArguments.FirstOrDefault(a => a.Key == "Persist");
		if (isPersistedArgument?.Value.Value is bool isPersistedValue)
		{
			isPersisted = isPersistedValue;
		}

		var (propertyKind, itemType) = GetPropertyKind(propertyTypeSymbol);
		string typeString = propertyTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		string itemTypeString = itemType.ToDisplayString();

		return propertyKind switch {
			PropertyKind.Scalar => new ScalarProperty(name, kubernetesName, typeString, isPersisted, persistencePriority),
			PropertyKind.Array => new ArrayProperty(name, kubernetesName, typeString, isPersisted, persistencePriority, IsPartial(propertyTypeSymbol), itemTypeString),
			PropertyKind.Dictionary => new DictionaryProperty(name, kubernetesName, typeString, isPersisted, persistencePriority, IsPartial(propertyTypeSymbol), itemTypeString),
			PropertyKind.Object => new ObjectProperty(name, kubernetesName, typeString, isPersisted, persistencePriority, IsPartial(propertyTypeSymbol)),
			_ => null
		};
	}

	private static bool IsPartial(INamedTypeSymbol typeSymbol)
	{
		return typeSymbol.DeclaringSyntaxReferences
			.Select(r => r.GetSyntax())
			.OfType<PropertyDeclarationSyntax>()
			.Any(p => p.Modifiers.Any(SyntaxKind.PartialKeyword));
	}

	private (PropertyKind, ITypeSymbol) GetPropertyKind(INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && typeSymbol.TypeArguments.Length == 1)
		{
			if (IsScalarType(typeSymbol.TypeArguments[0]))
			{
				return (PropertyKind.Scalar, typeSymbol.TypeArguments[0]);
			}

			return (PropertyKind.None, typeSymbol);
		}

		if (typeSymbol.Arity == 2 && SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, _dictionaryInterfaceType))
		{
			if (typeSymbol.TypeArguments[0].SpecialType != SpecialType.System_String)
			{
				return (PropertyKind.None, typeSymbol);
			}

			return (PropertyKind.Dictionary, typeSymbol.TypeArguments[1]);
		}

		if (typeSymbol.Arity == 1 && SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, _baseArrayType))
		{
			return (PropertyKind.Array, typeSymbol.TypeArguments[0]);
		}

		if (IsObject(typeSymbol))
		{
			return (PropertyKind.Object, typeSymbol);
		}

		return (PropertyKind.Scalar, typeSymbol);
	}

	private bool IsObject(ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is null)
		{
			return false;
		}

		if (typeSymbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _resourceAttributeType)))
		{
			return true;
		}

		while (typeSymbol is not null)
		{
			if (SymbolEqualityComparer.Default.Equals(typeSymbol, _baseObjectType))
			{
				return true;
			}

			typeSymbol = typeSymbol.BaseType;
		}

		return false;
	}

	private static bool IsScalarType(ITypeSymbol typeSymbol)
	{
		return typeSymbol.SpecialType
			is SpecialType.System_Boolean
			or SpecialType.System_Byte
			or SpecialType.System_SByte
			or SpecialType.System_Char
			or SpecialType.System_DateTime
			or SpecialType.System_Int16
			or SpecialType.System_Int32
			or SpecialType.System_Int64
			or SpecialType.System_UInt16
			or SpecialType.System_UInt32
			or SpecialType.System_UInt64
			or SpecialType.System_Single
			or SpecialType.System_Double
			or SpecialType.System_Decimal
			|| typeSymbol.TypeKind == TypeKind.Enum;
	}
	private enum PropertyKind
	{
		None = 0,
		Scalar,
		Array,
		Dictionary,
		Object,
	}

	private (string? apiVersion, string? kind) GetApiVersionAndKind()
	{
		var attributeType = semanticModel.Compilation.GetTypeByMetadataName(Constants.KubernetesResourceAttributeTypeName);
		var attribute = attributes.First(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));

		if (attribute.ConstructorArguments.Length == 0)
		{
			return (null, null);
		}

		return (attribute.ConstructorArguments[0].Value as string, attribute.ConstructorArguments[1].Value as string);
	}

	private bool IsMethodDeclared(string name)
	{
		return classSymbol
			.GetMembers(name)
			.OfType<IMethodSymbol>()
			.Any();
	}
}
