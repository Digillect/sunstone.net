using System.CodeDom.Compiler;
using Digillect.Sunstone.Resources.SourceGenerator.Generation;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;
using Microsoft.OpenApi.Models;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;

public static class OpenApiResourcesGenerator
{
	public static IEnumerable<KeyValuePair<ResourceIdentifier, string>> GenerateResources(
		IEnumerable<CustomResourceDefinition> crds,
		GeneratorConfiguration config)
	{
		return
			from crd in crds
			from schemaId in config.TopLevelResources
			from version in crd.Spec.Versions
			let schemaName = $"{crd.Spec.Group}.{version.Name}.{crd.Spec.Names.Kind}"
			where string.Equals(schemaId, schemaName, StringComparison.OrdinalIgnoreCase)
			let id = new ResourceIdentifier(schemaId)
			let generator = new ResourceGenerator(id, version.Schema!, config, _ => { })
			let sourceCode = generator.Generate()
			select new KeyValuePair<ResourceIdentifier, string>(id, sourceCode);
	}

	public static IEnumerable<KeyValuePair<ResourceIdentifier, string>> GenerateResources(OpenApiDocument document, GeneratorConfiguration config)
	{
		var processedSchemas = new HashSet<string>();
		var schemasToProcess = new HashSet<string>(config.TopLevelResources);

		while (schemasToProcess.Count > 0)
		{
			var schemasToProcessNextIteration = new HashSet<string>();

			void AddSchema(string schema)
			{
				if (processedSchemas.Contains(schema) || schemasToProcess.Contains(schema))
				{
					return;
				}

				schemasToProcessNextIteration.Add(schema);
			}

			foreach (string schemaId in schemasToProcess)
			{
				var schema = document.Components.Schemas[schemaId];

				processedSchemas.Add(schemaId);

				var id = new ResourceIdentifier(schemaId);
				var generator = new ResourceGenerator(id, schema, config, AddSchema);
				string sourceCode = generator.Generate();

				yield return new KeyValuePair<ResourceIdentifier, string>(id, sourceCode);
			}

			schemasToProcess = schemasToProcessNextIteration;
		}
	}

	public static string GenerateResourcesFactory(GeneratorConfiguration config)
	{
		var stringWriter = new StringWriter();
		var writer = new IndentedTextWriter(stringWriter);

		writer.WriteLine("namespace {0};", config.Namespace);
		writer.WriteLine();
		writer.WriteLine("public static class ResourcesCollectionExtensions");
		writer.WriteBlock(() => {
			foreach (string topLevelResourceId in config.TopLevelResources)
			{
				var id = new ResourceIdentifier(topLevelResourceId);

				string fullTypeName = $"{config.Namespace}.{id.FullTypeName}";

				writer.WriteLine(
					"public static {0} {1}(this ResourcesCollection resources, string name, Action<{0}>? configure = null) => resources.FindOrCreate(name, configure);",
					fullTypeName,
					id.GroupVersionKind.Kind);
			}
		});

		writer.WriteEmptyLine();
		writer.Flush();

		return stringWriter.ToString();
	}
}
