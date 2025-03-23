using System.Collections;

namespace Digillect.Sunstone;

public class BaseArray<T>(SunstoneObject? owner) : IReadOnlyList<T>
{
	private readonly List<T> _items = [];

	public int Count => _items.Count;

	public T this[int index] => _items[index];

	public IEnumerator<T> GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{
		if (item is SunstoneObject baseObject)
		{
			baseObject.SetParent(owner);
		}

		_items.Add(item);
	}

	public void Add(params T[] items)
	{
		foreach (var item in items)
		{
			if (item is SunstoneObject baseObject)
			{
				baseObject.SetParent(owner);
			}

			_items.Add(item);
		}
	}

	public T? AddOrReplace(T item, Func<T, bool> predicate)
	{
		var existing = _items.FirstOrDefault(predicate);

		if (existing is not null)
		{
			_items.Remove(existing);

			if (existing is SunstoneObject existingBaseObject)
			{
				existingBaseObject.SetParent(null);
			}
		}

		if (item is SunstoneObject baseObject)
		{
			baseObject.SetParent(owner);
		}

		_items.Add(item);

		return existing;
	}
}
