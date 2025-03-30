using System.Globalization;

namespace Digillect.Sunstone.Resources.Kubernetes;

public abstract class V1CoreConfigMapOrSecret<TSelf> : SunstoneObject
	where TSelf : V1CoreConfigMapOrSecret<TSelf>
{
	private readonly Dictionary<string, string> _data = new();
	private V1MetaObjectMeta? _metadata;

	public V1MetaObjectMeta Metadata => _metadata ??= CreateAndAdoptObject<V1MetaObjectMeta>();

	public IDictionary<string, string> Data => _data;

	public TSelf SetIf(bool condition, string key, string value)
	{
		if (condition)
		{
			Set(key, value);
		}

		return (TSelf) this;
	}

	public TSelf Set(string key, string value)
	{
		_data[key] = value;

		return (TSelf) this;
	}

	public TSelf SetIfNotNull(string key, string? value)
	{
		if (value is not null)
		{
			Set(key, value);
		}

		return (TSelf) this;
	}

	public TSelf Set(string key, bool value)
	{
		_data[key] = value ? "true" : "false";

		return (TSelf) this;
	}

	public TSelf SetIfNotNull(string key, bool? value)
	{
		if (value is not null)
		{
			Set(key, value.Value);
		}

		return (TSelf) this;
	}

	public TSelf Set(string key, double value)
	{
		_data[key] = value.ToString(CultureInfo.InvariantCulture);

		return (TSelf) this;
	}

	public TSelf SetIfNotNull(string key, double? value)
	{
		if (value is not null)
		{
			Set(key, value.Value);
		}

		return (TSelf) this;
	}

	public TSelf Set<T>(string key, T value)
		where T : notnull
	{
		string stringValue = value.ToString() ?? throw new ArgumentException("Unable to convert value to string", nameof(value));

		_data[key] = stringValue;

		return (TSelf) this;
	}

	public TSelf SetIfNotNull<T>(string key, T? value)
		where T : notnull
	{
		if (value is not null)
		{
			Set(key, value);
		}

		return (TSelf) this;
	}

	public TSelf SetIfNotNull<T>(string key, T? value)
		where T : struct
	{
		if (value is not null)
		{
			Set(key, value.Value);
		}

		return (TSelf) this;
	}

	protected virtual string ConvertValue(string value) => value;

	public override PersistableValuesCollection Persist()
	{
		return base.Persist()
			.Add("metadata", -80_000, _metadata)
			.Add("data", 50_000, _data.ToDictionary(kvp => kvp.Key, kvp => ConvertValue(kvp.Value)));
	}
}
