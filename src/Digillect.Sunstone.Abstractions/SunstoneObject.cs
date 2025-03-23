using System.ComponentModel;

namespace Digillect.Sunstone;

public abstract class SunstoneObject
{
	private SunstoneObject? _parent;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual PersistableValuesCollection Persist()
	{
		var result = new PersistableValuesCollection();

		return result;
	}

	#region Parentable
	[EditorBrowsable(EditorBrowsableState.Never)]
	public SunstoneObject? GetParent()
	{
		return _parent;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected internal void SetParent(SunstoneObject? parent)
	{
		_parent = parent;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected T Adopt<T>(T child)
		where T : SunstoneObject
	{
		child.SetParent(this);

		return child;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected T CreateAndAdoptObject<T>()
		where T : SunstoneObject, new()
	{
		var child = new T();

		child.SetParent(this);

		return child;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected BaseArray<T> CreateAndAdoptArray<T>()
	{
		var array = new BaseArray<T>(this);

		return array;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected Dictionary<string, T> CreateAndAdoptDictionary<T>()
	{
		return new Dictionary<string, T>();
	}
	#endregion
}
