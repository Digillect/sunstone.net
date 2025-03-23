using System.CodeDom.Compiler;
using CaseExtensions;
using Microsoft.OpenApi.Models;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;

public abstract class Property(OpenApiSchema schema, string name, bool isRequired)
{
	public OpenApiSchema Schema { get; } = schema;
	public string Name { get; } = name;
	public string PropertyName { get; } = name.ToPascalCase();
	public bool IsRequired { get; } = isRequired;

	public virtual string PersistenceMember => PropertyName;

	public abstract bool WriteFieldDeclaration(IndentedTextWriter writer);
	public abstract void WritePropertyDeclaration(IndentedTextWriter writer);
	public abstract bool WriteConstructorInitialization(IndentedTextWriter writer);

	public abstract void AddSchemasToProcess(Action<string> addSchema);

	public static Property? Create(string name, OpenApiSchema schema, bool isRequired, PropertyType? type = null)
	{
		type ??= PropertyType.Get(schema);

		return type switch
		{
			PropertyType.Scalar t => new ScalarProperty(schema, t, name, isRequired),
			PropertyType.Object t => new ObjectProperty(schema, t, name, isRequired),
			PropertyType.Array t => new ArrayProperty(schema, t, name, isRequired),
			PropertyType.Dictionary t => new DictionaryProperty(schema, t, name, isRequired),
			_ => null
		};
	}
}

public abstract class Property<TType>(OpenApiSchema schema, TType type, string name, bool isRequired)
	: Property(schema, name, isRequired)
	where TType : PropertyType
{
	public TType Type { get; } = type;

	public void WriteXmlComment(IndentedTextWriter writer)
	{
		XmlDocHelper.WriteDocComment(writer, Schema.Description, Name, PropertyName);
	}

	public override void AddSchemasToProcess(Action<string> addSchema)
	{
		Type.AddSchemasToProcessingList(addSchema);
	}

	public override bool WriteFieldDeclaration(IndentedTextWriter writer) => false;

	public override bool WriteConstructorInitialization(IndentedTextWriter writer) => false;
}

internal sealed class DictionaryProperty(OpenApiSchema schema, PropertyType.Dictionary type, string name, bool isRequired)
	: Property<PropertyType.Dictionary>(schema, type, name, isRequired)
{
	public override string PersistenceMember => $"_{Name}";

	public override bool WriteFieldDeclaration(IndentedTextWriter writer)
	{
		writer.WriteLine("private {0}? _{1};", Type.ImplementationType, Name);

		return true;
	}

	public override void WritePropertyDeclaration(IndentedTextWriter writer)
	{
		WriteXmlComment(writer);

		writer.WriteLine("public {0} {1} => _{2} ??= CreateAndAdoptDictionary<{3}>();", Type.ClrType, PropertyName, Name, Type.ValueType.ClrType);
	}
}

internal sealed class ObjectProperty(OpenApiSchema schema, PropertyType.Object type, string name, bool isRequired)
	: Property<PropertyType.Object>(schema, type, name, isRequired)
{
	public override string PersistenceMember => $"_{Name}";

	public override bool WriteFieldDeclaration(IndentedTextWriter writer)
	{
		writer.WriteLine("private {0}? _{1};", Type.ClrType, Name);

		return true;
	}

	public override void WritePropertyDeclaration(IndentedTextWriter writer)
	{
		WriteXmlComment(writer);

		writer.WriteLine("public {0} {1} => _{2} ??= CreateAndAdoptObject<{0}>();", Type.ClrType, PropertyName, Name);
	}
}

internal sealed class ScalarProperty(OpenApiSchema schema, PropertyType.Scalar type, string name, bool isRequired)
	: Property<PropertyType.Scalar>(schema, type, name, isRequired)
{
	public override void WritePropertyDeclaration(IndentedTextWriter writer)
	{
		WriteXmlComment(writer);

		writer.Write("public {0}", Type.ClrType);

		if (!IsRequired)
		{
			writer.Write("?");
		}

		writer.Write(" {0} {{ get; set; }}", PropertyName);

		if (IsRequired && Type.ClrType == "string")
		{
			writer.Write(" = default!;");
		}

		writer.WriteLine();
	}
}

internal sealed class ArrayProperty(OpenApiSchema schema, PropertyType.Array type, string name, bool isRequired)
	: Property<PropertyType.Array>(schema, type, name, isRequired)
{
	public override string PersistenceMember => $"_{Name}";

	public override bool WriteFieldDeclaration(IndentedTextWriter writer)
	{
		writer.WriteLine("private {0}? _{1};", Type.ClrType, Name);

		return true;
	}

	public override void WritePropertyDeclaration(IndentedTextWriter writer)
	{
		WriteXmlComment(writer);

		writer.WriteLine("public {0} {1} => _{2} ??= CreateAndAdoptArray<{3}>();", Type.ClrType, PropertyName, Name, Type.ItemType.ClrType);
	}
}
