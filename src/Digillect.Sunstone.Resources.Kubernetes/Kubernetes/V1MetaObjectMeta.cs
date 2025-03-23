namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1MetaObjectMeta
{
	public V1MetaObjectMeta AddLabel(string name, string value)
	{
		Labels.Add(name, value);

		return this;
	}

	public V1MetaObjectMeta AddAnnotation(string name, string value)
	{
		Annotations.Add(name, value);

		return this;
	}
}