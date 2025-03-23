using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using SharpYaml.Serialization;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;

public static class CrdParser
{
	public static Result<IReadOnlyCollection<CustomResourceDefinition>> ParseCrd(string sourceText)
	{
		using var reader = new StringReader(sourceText);

		YamlStream stream = [];

		try
		{
			stream.Load(reader);
		}
		catch (Exception ex)
		{
			return Error.Generic(ex.Message);
		}

		List<CustomResourceDefinition> crdList = [];
		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		foreach (var yamlDocument in stream.Documents)
		{
			var jsonNode = yamlDocument.ToJsonNode();

			var crd = jsonNode.Deserialize<CustomResourceDefinition>(options);

			if (crd is not null)
			{
				crdList.Add(crd);
			}
		}

		return crdList;
	}
}

public class CustomResourceDefinition
{
	public string ApiVersion { get; set; } = null!;
	public string Kind { get; set; } = null!;
	public CustomResourceDefinitionSpec Spec { get; set; } = null!;
}

public class CustomResourceDefinitionSpec
{
	public string Group { get; set; } = null!;
	public CustomResourceDefinitionNames Names { get; set; } = null!;
	public string Scope { get; set; } = null!;
	public IReadOnlyList<CustomResourceDefinitionVersion> Versions { get; set; } = null!;
}

public class CustomResourceDefinitionNames
{
	public IReadOnlyList<string>? Categories { get; set; }
	public string Kind { get; set; } = null!;
	public string? ListKind { get; set; }
	public string Plural { get; set; } = null!;
	public string? Singular { get; set; }
	public IReadOnlyList<string>? ShortNames { get; set; }
}

public class CustomResourceDefinitionVersion
{
	public string Name { get; set; } = null!;
	[JsonConverter(typeof(ObjectToOpenApiSchemaConverter))]
	public OpenApiSchema? Schema { get; set; }
	public bool Served { get; set; }
	public bool Storage { get; set; }
}

public class ObjectToOpenApiSchemaConverter : JsonConverter<OpenApiSchema>
{
	public override OpenApiSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var jsonNode = JsonNode.Parse(ref reader);
		if (jsonNode is not JsonObject jsonObject)
		{
			return null;
		}

		if (!jsonObject.TryGetPropertyValue("openAPIV3Schema", out var openApiSchemaNode))
		{
			return null;
		}

		string text = openApiSchemaNode!.ToJsonString();
		var openApiReader = new OpenApiStringReader();

		var schema = openApiReader.ReadFragment<OpenApiSchema>(text, OpenApiSpecVersion.OpenApi3_0, out var openApiDiagnostic);

		if (openApiDiagnostic?.Errors.Count > 0)
		{
			throw new Exception(openApiDiagnostic.Errors[0].Message);
		}

		return schema;
	}

	public override void Write(Utf8JsonWriter writer, OpenApiSchema value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
