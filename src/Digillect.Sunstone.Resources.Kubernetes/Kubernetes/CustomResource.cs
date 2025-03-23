using System.ComponentModel;

namespace Digillect.Sunstone.Resources.Kubernetes;

public abstract class CustomResource<T> : SunstoneObject, IHaveApiVersionAndKind, IHaveMetadata, IHaveResourceName
	where T : SunstoneObject, IHaveFactory<T>
{
	private V1MetaObjectMeta? _metadata;

	public string ApiVersion => T.ResourceApiVersion;
	public string Kind => T.ResourceKind;

	[EditorBrowsable(EditorBrowsableState.Never)]
	string? IHaveResourceName.ResourceName => Metadata.Name;

	/// <summary>
	/// ObjectMeta is Metadata that all persisted resources must have, which includes all objects users must create.
	/// </summary>
	public V1MetaObjectMeta Metadata => _metadata ??= CreateAndAdoptObject<V1MetaObjectMeta>();

	public override PersistableValuesCollection Persist()
	{
		return base.Persist()
			.Add("apiVersion", -100_000, ApiVersion)
			.Add("kind", -90_000, Kind)
			.Add("metadata", -80_000, _metadata);
	}
}
