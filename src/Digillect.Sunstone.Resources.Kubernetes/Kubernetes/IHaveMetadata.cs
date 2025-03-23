namespace Digillect.Sunstone.Resources.Kubernetes;

public interface IHaveMetadata
{
	public V1MetaObjectMeta Metadata { get; }
}
