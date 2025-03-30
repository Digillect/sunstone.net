using System.Text.Json;

namespace Digillect.Sunstone.Serialization;

public class JsonPersistedObjectSerializer : IPersistedObjectSerializer
{
	private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

	public string FileExtension => "json";

	public string Serialize(PersistableValuesCollection persistableValues)
	{
		return JsonSerializer.Serialize(persistableValues.ToDictionary(), _options);
	}
}
