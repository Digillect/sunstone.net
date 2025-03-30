namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CorePodSpec
{
	public V1CorePodSpec Container(string name, Action<V1CoreContainer> config) => CreateContainer(Containers, name, config);

	public V1CorePodSpec InitContainer(string name, Action<V1CoreContainer> config) => CreateContainer(InitContainers, name, config);

	public V1CorePodSpec MainContainer(Action<V1CoreContainer> config) => Container("main", config);

	private V1CorePodSpec CreateContainer(BaseArray<V1CoreContainer> containers, string name, Action<V1CoreContainer> config)
	{
		var container = containers.FirstOrDefault(c => c.Name == name);

		if (container is null)
		{
			container = new V1CoreContainer
			{
				Name = name
			};

			containers.Add(container);
		}

		config(container);

		return this;
	}
}
