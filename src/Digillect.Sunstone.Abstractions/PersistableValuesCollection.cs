namespace Digillect.Sunstone;

public class PersistableValuesCollection
{
	private readonly List<Entry> _values = [];

	private record Entry(string Name, int Order, object Value);

	public PersistableValuesCollection Add(string name, int order, IPersistableValue value)
	{
		object? persistedValue = value.GetPersistableValue();

		if (persistedValue is not null)
		{
			_values.Add(new Entry(name, order, persistedValue));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, string? value)
	{
		if (value is not null)
		{
			_values.Add(new Entry(name, order, value));
		}

		return this;
	}

	public PersistableValuesCollection Add<T>(string name, int order, T value)
		where T : struct, Enum
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add<T>(string name, int order, T? value)
		where T : struct, Enum
	{
		if (value is not null)
		{
			_values.Add(new Entry(name, order, value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, bool value)
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, bool? value)
	{
		if (value.HasValue)
		{
			_values.Add(new Entry(name, order, value.Value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, int value)
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, int? value)
	{
		if (value.HasValue)
		{
			_values.Add(new Entry(name, order, value.Value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, long value)
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, long? value)
	{
		if (value.HasValue)
		{
			_values.Add(new Entry(name, order, value.Value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, float value)
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, float? value)
	{
		if (value.HasValue)
		{
			_values.Add(new Entry(name, order, value.Value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, double value)
	{
		_values.Add(new Entry(name, order, value));

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, double? value)
	{
		if (value.HasValue)
		{
			_values.Add(new Entry(name, order, value.Value));
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, SunstoneObject? value)
	{
		if (value is not null)
		{
			var persisted = value.Persist();

			if (persisted._values.Count > 0)
			{
				_values.Add(new Entry(name, order, persisted.ToDictionary()));
			}
		}

		return this;
	}

	public PersistableValuesCollection Add(string name, int order, IDictionary<string, string>? value)
	{
		if (value is { Count: > 0 })
		{
			_values.Add(new Entry(name, order, value));
		}

		return this;
	}

	public PersistableValuesCollection Add<T>(string name, int order, IDictionary<string, T>? value)
	{
		if (value is { Count: > 0 })
		{
			_values.Add(new Entry(name, order, value));
		}

		return this;
	}

	public PersistableValuesCollection Add<T>(string name, int order, IReadOnlyList<T>? value)
	{
		if (value is null || value.Count == 0)
		{
			return this;
		}

		object[] result = value.Where(e => e is not null).Select(ConvertValue).ToArray()!;

		if (result.Length > 0)
		{
			_values.Add(new Entry(name, order, result));
		}

		return this;

		object? ConvertValue(T v)
		{
			return v switch {
				SunstoneObject baseObject => baseObject.Persist().ToDictionary(),
				IPersistableValue persistedValue => persistedValue.GetPersistableValue(),
				_ => v
			};
		}
	}

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		return _values
			.OrderBy(e => e.Order)
			.ThenBy(e => e.Name)
			.ToDictionary(e => e.Name, e => e.Value);
	}
}
