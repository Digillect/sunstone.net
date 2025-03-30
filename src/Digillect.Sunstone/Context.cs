namespace Digillect.Sunstone;

public class Context<TValues>(ResourcesCollection resources, TValues values)
	where TValues : notnull
{
    public ResourcesCollection Resources { get; } = resources;
	public TValues Values { get; } = values;

	public void Deconstruct(out ResourcesCollection resources, out TValues values)
    {
        resources = Resources;
        values = Values;
    }

	public Context<TValues> AddComponent<TComponent>()
        where TComponent : IComponent<TValues>, new()
	{
		var component = new TComponent();

        component.AddResources(this);

        return this;
    }
}

public static class Context
{
	public static Context<TValues> Create<TValues>(TValues values)
		where TValues : notnull
	{
		return new Context<TValues>(new ResourcesCollection(), values);
	}
}
