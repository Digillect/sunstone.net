namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CoreContainer
{
	private EnvironmentHelper? _environmentHelper;

	/// <summary>
	/// Provides helpers to manage container environment.
	/// </summary>
	/// <param name="configure">Action to setup container environment.</param>
	/// <returns>Container to continue configuration.</returns>
	public V1CoreContainer Environment(Action<EnvironmentHelper> configure)
	{
		_environmentHelper ??= new EnvironmentHelper(this, Env, EnvFrom);

		configure(_environmentHelper);

		return this;
	}

	public V1CoreContainer ExposePort(
		int containerPort,
		string? hostIp = null,
		int? hostPort = null,
		string? name = null,
		string? protocol = null,
		Action<V1CoreContainerPort>? configure = null)
	{
		var port = new V1CoreContainerPort {
			ContainerPort = containerPort,
			HostIP = hostIp,
			HostPort = hostPort,
			Name = name,
			Protocol = protocol
		};

		configure?.Invoke(port);

		Ports.Add(port);

		return this;
	}

	public V1CoreContainer ExposeHttpPort(int port, Action<V1CoreContainerPort>? configure = null)
	{
		return ExposePort(port, name: NameOrPort.HttpPortName, configure: configure);
	}
}
