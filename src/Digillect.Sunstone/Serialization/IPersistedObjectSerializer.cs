namespace Digillect.Sunstone.Serialization;

public interface IPersistedObjectSerializer
{
	string FileExtension { get; }

	string Serialize(PersistableValuesCollection persistableValues);
}
