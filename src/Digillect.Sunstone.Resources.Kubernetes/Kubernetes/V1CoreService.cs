namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CoreService : ICanMatchLabels
{
	/// <inheritdoc cref="ICanMatchLabels.MatchLabel"/>
	public void MatchLabel(string label, string value)
	{
		Metadata.Labels.Add(label, value);
		Spec.Selector.Add(label, value);
	}

	public V1CoreService ExposePort(int port, string name, Action<V1CoreServicePort>? configure = null)
	{
		var servicePort = new V1CoreServicePort {
			Port = port,
			Name = name
		};

		configure?.Invoke(servicePort);

		Spec.Ports.Add(servicePort);

		return this;
	}

	public V1CoreService ExposeHttpPort(int port, Action<V1CoreServicePort>? configure = null) =>
		ExposePort(port, "http", configure);
}
