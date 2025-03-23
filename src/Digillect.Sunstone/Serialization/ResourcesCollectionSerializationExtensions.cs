namespace Digillect.Sunstone.Serialization;

public static class ResourcesCollectionSerializationExtensions
{
	public static void Serialize<TSerializer>(this ResourcesCollection resources, string basePath)
		where TSerializer : IPersistedObjectSerializer, new()
	{
		var serializer = new TSerializer();

		resources.Serialize(serializer, basePath);
	}

	public static void Serialize(this ResourcesCollection resources, IPersistedObjectSerializer serializer, string basePath)
	{
		if (Directory.Exists(basePath))
		{
			Directory.Delete(basePath, true);
		}

		Directory.CreateDirectory(basePath);

		int anonymousResourceCount = 0;

		foreach (var resource in resources.GetResources())
		{
			var persisted = resource.Persist();
			string result = serializer.Serialize(persisted);
			string? resourceName = resource is IHaveResourceName resourceWithName ? resourceWithName.ResourceName : null;

			resourceName ??= (++anonymousResourceCount).ToString();

			string kind = resource is IHaveApiVersionAndKind resourceWithApiVersionAndKind ? resourceWithApiVersionAndKind.Kind : "unknown";

			string fileName = $"{resourceName.ToLowerInvariant()}-{kind.ToLowerInvariant()}.{serializer.FileExtension}";

			File.WriteAllText(Path.Combine(basePath, fileName), result);
		}
	}
}
