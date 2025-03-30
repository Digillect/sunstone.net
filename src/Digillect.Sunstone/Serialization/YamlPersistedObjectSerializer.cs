using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Digillect.Sunstone.Serialization;

public class YamlPersistedObjectSerializer : IPersistedObjectSerializer
{
	public string FileExtension => "yaml";

	public string Serialize(PersistableValuesCollection persistableValues)
	{
		var serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.WithQuotingNecessaryStrings()
			.Build();

		using var sw = new StringWriter();

		serializer.Serialize(sw, persistableValues.ToDictionary());

		return sw.ToString();
	}
}
