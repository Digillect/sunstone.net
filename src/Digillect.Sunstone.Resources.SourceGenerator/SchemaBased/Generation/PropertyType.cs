using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;
using Microsoft.OpenApi.Models;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;

public abstract class PropertyType
{
	public abstract string ClrType { get; }

	public virtual void AddSchemasToProcessingList(Action<string> addSchema)
	{
	}

	public static PropertyType? Get(PropertyTypeConfiguration configuration)
	{
		return configuration.Array is not null
			? new Array(configuration.Array)
			: configuration.Dictionary is not null
				? new Dictionary(configuration.Dictionary)
				: configuration.Object is not null
					? new Object(configuration.Object)
					: configuration.Scalar is not null
						? new Scalar(configuration.Scalar)
						: null;
	}

	public static PropertyType? Get(OpenApiSchema schema)
	{
		if (Scalar.IsScalarProperty(schema))
		{
			return new Scalar(schema);
		}

		if (schema.Type == "object")
		{
			if (schema.Reference is null)
			{
				if (!string.IsNullOrEmpty(schema.AdditionalProperties?.Type))
				{
					return new Dictionary(schema);
				}
			}
			else
			{
				return new Object(schema.Reference.Id);
			}
		}

		if (schema.Type == "array")
		{
			return new Array(schema);
		}

		return null;
	}

	public sealed class Dictionary : PropertyType
	{
		public Dictionary(OpenApiSchema schema)
		{
			ValueType = Get(schema.AdditionalProperties)
						?? throw new InvalidOperationException($"Unknown dictionary value type: {schema.AdditionalProperties}");
		}

		public Dictionary(DictionaryPropertyTypeConfiguration config)
		{
			ValueType = Get(config.ValueType) ?? throw new InvalidOperationException($"Unknown dictionary value type: {config.ValueType}");
		}

		public override string ClrType => DeclarationType;

		public PropertyType ValueType { get; }

		public string KeyType => "string";
		public string DeclarationType => $"IDictionary<{KeyType}, {ValueType.ClrType}>";
		public string ImplementationType => $"Dictionary<{KeyType}, {ValueType.ClrType}>";
	}

	public sealed class Array : PropertyType
	{
		public Array(OpenApiSchema schema)
		{
			if (!string.IsNullOrEmpty(schema.Items.Type))
			{
				ItemType = Get(schema.Items)!;
			}
			else if (schema.Items.Reference is not null)
			{
				ItemType = Get(schema.Items.Reference.HostDocument.Components.Schemas[schema.Items.Reference.Id])!;
			}
			else
			{
				throw new InvalidOperationException($"Unknown array item type: {schema}");
			}
		}

		public Array(ArrayPropertyTypeConfiguration config)
		{
			ItemType = Get(config.ItemType) ?? throw new InvalidOperationException($"Unknown array item type: {config.ItemType}");
		}

		public override string ClrType => $"BaseArray<{ItemType.ClrType}>";

		public PropertyType ItemType { get; }

		public override void AddSchemasToProcessingList(Action<string> addSchema)
		{
			ItemType.AddSchemasToProcessingList(addSchema);
		}
	}

	public sealed class Object : PropertyType
	{
		private readonly string? _additionalSchemaId;

		public Object(string schemaId)
		{
			var id = new ResourceIdentifier(schemaId);

			ClrType = id.FullTypeName;
			_additionalSchemaId = id.SchemaId;
		}

		public Object(ObjectPropertyTypeConfiguration config)
		{
			ClrType = config.ClrType;
			_additionalSchemaId = config.AdditionalSchemaId;
		}

		public override string ClrType { get; }

		public override void AddSchemasToProcessingList(Action<string> addSchema)
		{
			if (_additionalSchemaId is not null)
			{
				addSchema(_additionalSchemaId);
			}
		}
	}

	public sealed class Scalar : PropertyType
	{
		public Scalar(OpenApiSchema schema)
		{
			ClrType = (schema.Type, schema.Format) switch {
				("string", _) => "string",
				("boolean", _) => "bool",
				("integer", "int32") => "int",
				("integer", "int64") => "long",
				("number", "double") => "double",
				("number", _) => "float",
				({ } type, _) => throw new InvalidOperationException($"Unknown scalar property type: {type}")
			};
		}

		public Scalar(ScalarPropertyTypeConfiguration config)
		{
			ClrType = config.ClrType;
		}

		public override string ClrType { get; }

		public static bool IsScalarProperty(OpenApiSchema schema)
		{
			string type = schema.Type;

			return type is "string" or "boolean" or "integer" or "number";
		}
	}
}
