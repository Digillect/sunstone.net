namespace Digillect.Sunstone.Resources.Kubernetes;

public readonly struct NameOrPort : IPersistableValue
{
	public const string HttpPortName = "http";

	public static readonly NameOrPort Http = new();

	public readonly int? Port;
	public readonly string? Name;

	public NameOrPort()
	{
		Name = HttpPortName;
	}

	private NameOrPort(int port)
	{
		Port = port;
	}

	private NameOrPort(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		Name = name;
	}

	public static implicit operator NameOrPort(int port) => new(port);
	public static implicit operator NameOrPort(string name) => new(name);

	object? IPersistableValue.GetPersistableValue()
	{
		if (Name is not null)
		{
			return Name;
		}

		return Port;
	}
}
