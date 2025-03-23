using System.ComponentModel;

namespace Digillect.Sunstone;

public class ResourcesCollection
{
	private readonly Dictionary<ResourceKey, SunstoneObject> _resources = new();
	private readonly List<Action<SunstoneObject>> _initializers = [];

	private record ResourceKey(string Name, string ApiVersion, string Kind);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public T FindOrCreate<T>(string name, Action<T>? configure = null)
		where T : SunstoneObject, IHaveFactory<T>
	{
		var resourceKey = new ResourceKey(name, T.ResourceApiVersion, T.ResourceKind);

		if (!_resources.TryGetValue(resourceKey, out var resource))
		{
			resource = T.Create(name, this);

			foreach (var initializer in _initializers)
			{
				initializer(resource);
			}

			_resources.Add(resourceKey, resource);

			configure?.Invoke((T) resource);
		}

		return (T) resource;
	}

	public ResourcesCollection AddInitializer(Action<SunstoneObject> initializer)
	{
		_initializers.Add(initializer);

		return this;
	}

	public ResourcesCollection AddInitializer<T>(Action<T> initializer)
	{
		_initializers.Add(resource => {
			if (resource is T casted)
			{
				initializer(casted);
			}
		});

		return this;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public IReadOnlyCollection<SunstoneObject> GetResources() => _resources.Values;
}
