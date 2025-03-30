using System.Diagnostics;
using CaseExtensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;

public class GroupVersionKind(string group, string version, string kind)
{
	public string Group => group;
	public string Version => version;
	public string Kind => kind;

	public string ApiVersion => group is "" or "core" ? version : $"{group}/{version}";

	public static GroupVersionKind FromIdentifier(string identifier)
	{
		string[] parts = identifier.Split('.');

		return new GroupVersionKind(parts[0], parts[1], parts[2]);
	}

	public static GroupVersionKind FromSchemaExtension(IOpenApiExtension extension)
	{
		if (extension is not OpenApiArray { Count: 1 } array)
		{
			throw new InvalidGroupVersionKindExtensionException("Extension is not an array with a single element");
		}

		if (array[0] is not OpenApiObject obj)
		{
			throw new InvalidGroupVersionKindExtensionException("Extension element is not an object");
		}

		string group = Get("group") ?? throw new InvalidGroupVersionKindExtensionException("Extension element does not contain group");
		string version = Get("version") ?? throw new InvalidGroupVersionKindExtensionException("Extension element does not contain version");
		string kind = Get("kind") ?? throw new InvalidGroupVersionKindExtensionException("Extension element does not contain kind");

		return new GroupVersionKind(group == "" ? "core" : group, version, kind);

		string? Get(string key) => (obj[key] as OpenApiString)?.Value;
	}
}

public class InvalidGroupVersionKindExtensionException(string message) : Exception(message);

[DebuggerDisplay("{SchemaId}")]
public class ResourceIdentifier
{
	private static readonly string[] Prefixes = ["io.k8s.api.", "io.k8s.apimachinery.pkg.apis."];

	public ResourceIdentifier(string schemaId)
	{
		SchemaId = schemaId;
		ShortSchemaId = GetShortId(schemaId);

		GroupVersionKind = GroupVersionKind.FromIdentifier(ShortSchemaId);

		Namespace = $"{GroupVersionKind.Version.ToPascalCase()}{GroupVersionKind.Group.ToPascalCase()}";
		TypeName = $"{Namespace}{GroupVersionKind.Kind}";
	}

	public string SchemaId { get; }
	public string ShortSchemaId { get; }

	public GroupVersionKind GroupVersionKind { get; }

	public string Namespace { get; }
	public string TypeName { get; }
	public string FullTypeName => TypeName; // $"{Namespace}.{TypeName}";

	private static string GetShortId(string id)
	{
		foreach (string prefix in Prefixes)
		{
			if (id.StartsWith(prefix))
			{
				return id.Substring(prefix.Length);
			}
		}

		return id;
	}
}
