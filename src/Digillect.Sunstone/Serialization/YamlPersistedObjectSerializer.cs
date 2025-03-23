namespace Digillect.Sunstone.Serialization;

public class YamlPersistedObjectSerializer : IPersistedObjectSerializer
{
	public string FileExtension => "yaml";

	public string Serialize(PersistableValuesCollection persistableValues)
	{
		var settings = new SharpYaml.Serialization.SerializerSettings {
			NamingConvention = new SharpYaml.Serialization.CamelCaseNamingConvention()
		};

		var serializer = new SharpYaml.Serialization.Serializer(settings);

		using var sw = new StringWriter();

		serializer.Serialize(sw, persistableValues.ToDictionary());

		return sw.ToString();
	}
}
