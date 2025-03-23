namespace Digillect.Sunstone;

public interface IComponent<TValues>
    where TValues : notnull
{
    void AddResources(Context<TValues> context);
}
