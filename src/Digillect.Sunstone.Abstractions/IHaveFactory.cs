namespace Digillect.Sunstone;

public interface IHaveFactory<out T>
	where T : SunstoneObject
{
	public static abstract string ResourceApiVersion { get; }
	public static abstract string ResourceKind { get; }

	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public ResourcesCollection Resources { get; }

	public static abstract T Create(string name, ResourcesCollection resources);
}
