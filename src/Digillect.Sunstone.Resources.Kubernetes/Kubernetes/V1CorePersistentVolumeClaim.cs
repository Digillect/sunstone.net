namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CorePersistentVolumeClaim : ICanMatchLabels
{
	/// <inheritdoc cref="ICanMatchLabels.MatchLabel"/>
	public void MatchLabel(string label, string value)
	{
		Spec.Selector.Equals(label, value);
	}
}
