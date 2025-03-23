using System.CodeDom.Compiler;
using Digillect.Sunstone.Resources.SourceGenerator.Generation;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;
using Microsoft.OpenApi.Models;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;

public class ResourceGenerator
{
	private readonly ResourceIdentifier _id;
	private readonly OpenApiSchema _schema;
	private readonly GeneratorConfiguration _config;
	private readonly ResourceConfiguration _resourceConfig;
	private readonly GroupVersionKind _groupVersionKind;
	private readonly List<Property> _properties;
	private readonly bool _isTopLevel;

	public ResourceGenerator(ResourceIdentifier id, OpenApiSchema schema, GeneratorConfiguration config, Action<string> addSchema)
	{
		_id = id;
		_schema = schema;
		_config = config;

		_config.Resources.TryGetValue(_id.ShortSchemaId, out var resourceConfig);

		_resourceConfig = resourceConfig ?? ResourceConfiguration.Default;

		_groupVersionKind = _schema.Extensions.TryGetValue("x-kubernetes-group-version-kind", out var extension)
			? GroupVersionKind.FromSchemaExtension(extension)
			: _id.GroupVersionKind;

		(_properties, _isTopLevel) = ProcessProperties(addSchema);
	}

	public string Generate()
	{
		var stringWriter = new StringWriter();
		var writer = new IndentedTextWriter(stringWriter);

		WriteHeaderComment(writer);
		WriteClassDeclaration(writer);

		writer.WriteBlock(() => {
			WriteFieldDeclarations(writer);
			WriteConstructor(writer);

			if (_isTopLevel)
			{
				WriteApiVersionAndKind(writer);
				WriteResourceName(writer);
				WriteResourcesCollection(writer);
				WriteResourceFactory(writer);
			}

			WritePropertyDeclarations(writer);
			WritePersist(writer);
		});

		writer.Flush();

		return stringWriter.ToString();
	}

	private void WritePersist(IndentedTextWriter writer)
	{
		writer.WriteEmptyLine();

		writer.WriteLine("public override PersistableValuesCollection Persist()");
		writer.WriteBlock(() => {
			writer.Write("return base.Persist()");
			writer.Indent++;

			IEnumerable<Property> properties = _properties;

			if (_isTopLevel)
			{
				properties = WriteTopLevelResourcePersistence(writer, properties);
			}

			WritePropertiesPersistence(writer, properties);

			writer.WriteLine(";");
			writer.Indent--;
		});
	}

	private IEnumerable<Property> WriteTopLevelResourcePersistence(IndentedTextWriter writer, IEnumerable<Property> properties)
	{
		writer.WriteLine();
		writer.WriteLine(".Add(\"apiVersion\", -100_000, ApiVersion)");
		writer.Write(".Add(\"kind\", -90_000, Kind)");

		// TODO: Should we check for IHaveMetadata?
		if (!_resourceConfig.ShouldIgnoreProperty("metadata"))
		{
			writer.WriteLine();
			writer.Write(".Add(\"metadata\", -80_000, _metadata)");

			properties = properties.Where(e => e.Name != "metadata");
		}

		return properties;
	}

	private void WritePropertiesPersistence(IndentedTextWriter writer, IEnumerable<Property> properties)
	{
		foreach (var property in properties)
		{
			int order = -1;

			for (int index = 0; index < _resourceConfig.PropertiesPriority.Count; ++index)
			{
				if (_resourceConfig.PropertiesPriority[index] == property.Name)
				{
					order = index;
					break;
				}
			}

			writer.WriteLine();
			writer.Write(".Add(\"{0}\", {1}, {2})", property.Name, order == -1 ? 100_000 : order, property.PersistenceMember);
		}
	}

	private void WriteResourceFactory(IndentedTextWriter writer)
	{
		writer.WriteEmptyLine();
		writer.WriteLine("public static {0} Create(string name, ResourcesCollection resources)", _id.TypeName);
		writer.WriteBlock(() => {
			writer.WriteLine("var resource = new {0}", _id.TypeName);
			writer.WriteBlock(() => {
				writer.WriteLine("Resources = resources,");
				writer.WriteLine("Metadata =");
				writer.WriteBlock(() => {
					writer.WriteLine("Name = name");
				});
			}, withSemicolon: true);

			writer.WriteEmptyLine();
			writer.WriteLine("return resource;");
		});
	}

	private void WriteApiVersionAndKind(IndentedTextWriter writer)
	{
		writer.WriteEmptyLine();
		writer.WriteLine($"public static string ResourceApiVersion => \"{_groupVersionKind.ApiVersion}\";");
		writer.WriteLine($"public static string ResourceKind => \"{_groupVersionKind.Kind}\";");
		writer.WriteLine();
		writer.WriteLine("public string ApiVersion => ResourceApiVersion;");
		writer.WriteLine("public string Kind => ResourceKind;");
	}

	private void WriteResourceName(IndentedTextWriter writer)
	{
		writer.WriteEmptyLine();
		writer.WriteLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
		writer.WriteLine("string? IHaveResourceName.ResourceName => Metadata?.Name;");
	}

	private void WriteResourcesCollection(IndentedTextWriter writer)
	{
		writer.WriteLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
		writer.WriteLine("public required ResourcesCollection Resources { get; set; }");
		writer.WriteEmptyLine();
	}

	private static void WriteHeaderComment(IndentedTextWriter writer)
	{
		writer.WriteLine("// <auto-generated>");
		writer.WriteLine("//     This code was generated by the Digillect.Sunstone.Resources.Generator tool.");
		writer.WriteLine("//");
		writer.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
		writer.WriteLine("//     the code is regenerated.");
		writer.WriteLine("// </auto-generated>");
		writer.WriteEmptyLine();
		writer.WriteLine("#nullable enable");
		writer.WriteEmptyLine();
	}

	private void WriteClassDeclaration(IndentedTextWriter writer)
	{
		//string fullTypeName = $"{_config.NamespaceBase}.{_id.Namespace}.{_id.TypeName}";
		string fullTypeName = $"{_config.Namespace}.{_id.TypeName}";

		//writer.WriteLine("namespace {0}.{1};", _config.NamespaceBase, _id.Namespace);
		writer.WriteLine("namespace {0};", _config.Namespace);
		writer.WriteEmptyLine();

		XmlDocHelper.WriteDocComment(writer, _schema.Description);

		writer.Write("public sealed partial class {0} : {1}", _id.TypeName, string.Format(_resourceConfig.BaseClass, fullTypeName));

		if (_isTopLevel)
		{
			writer.Write(", IHaveFactory<{0}>, IHaveApiVersionAndKind, IHaveMetadata, IHaveResourceName", fullTypeName);
		}

		if (_resourceConfig.AdditionalInterfaces.Count > 0)
		{
			writer.Write(", {0}", string.Join(", ", _resourceConfig.AdditionalInterfaces));
		}

		writer.WriteLine();
	}

	private void WriteConstructor(IndentedTextWriter writer)
	{
		writer.WriteLine("public {0}()", _id.TypeName);

		// if (isTopLevel)
		// {
		// 	writer.WriteLine("(string name) : base(name)");
		// }
		// else
		// {
		// 	writer.WriteLine("()");
		// }

		writer.WriteBlock(() => {
			bool hasPropertiesInitializer = false;

			foreach (var property in _properties)
			{
				hasPropertiesInitializer |= property.WriteConstructorInitialization(writer);
			}

			if (hasPropertiesInitializer)
			{
				writer.WriteLine();
			}

			writer.WriteLine("SetupResource();");
		});

		writer.WriteEmptyLine();
		writer.WriteLine("partial void SetupResource();");
	}

	private void WriteFieldDeclarations(IndentedTextWriter writer)
	{
		bool hasFieldDeclarations = false;

		foreach (var property in _properties)
		{
			hasFieldDeclarations |= property.WriteFieldDeclaration(writer);
		}

		if (hasFieldDeclarations)
		{
			writer.WriteLine();
		}
	}

	private void WritePropertyDeclarations(IndentedTextWriter writer)
	{
		foreach (var property in _properties)
		{
			writer.WriteLine();
			property.WritePropertyDeclaration(writer);
		}
	}

	private (List<Property>, bool) ProcessProperties(Action<string> addSchema)
	{
		bool haveApiVersion = false, haveKind = false, haveMetadata = false;
		var properties = new List<Property>();

		foreach (var kvp in _schema.Properties)
		{
			string name = kvp.Key;
			OpenApiSchema? schema = kvp.Value;

			_resourceConfig.Properties.TryGetValue(name, out var propertyConfig);

			propertyConfig ??= PropertyConfiguration.Default;

			switch (name)
			{
				case "apiVersion":
					haveApiVersion = true;
					continue;

				case "kind":
					haveKind = true;
					continue;

				case "metadata":
					haveMetadata = true;
					break;

				case "status":
					continue;
			}

			if (_resourceConfig.ShouldIgnoreProperty(name) || schema.ReadOnly)
			{
				continue;
			}

			bool isRequired = _schema.Required.Contains(name);

			// if (name == "metadata" && _groupVersionKind is not null)
			// {
			// 	isRequired = true;
			// }

			if (propertyConfig.Required is not null)
			{
				isRequired = propertyConfig.Required.Value;
			}

			PropertyType? type = null;
			if (propertyConfig.Type is not null)
			{
				type = PropertyType.Get(propertyConfig.Type);
			}

			var property = Property.Create(name, schema, isRequired, type);

			if (property is not null)
			{
				property.AddSchemasToProcess(addSchema);

				properties.Add(property);
			}
		}

		return (properties, haveApiVersion && haveKind && haveMetadata);
	}
}
