namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1AppsDeployment : IWorkload, ICanMatchLabels
{
	/// <inheritdoc cref="IWorkload.PodTemplate"/>
	public V1CorePodTemplateSpec PodTemplate => Spec.Template;

	/// <inheritdoc cref="ICanMatchLabels.MatchLabel"/>
	public void MatchLabel(string label, string value)
	{
		Spec.Selector.Equals(label, value);
		Spec.Template.Metadata.Labels.Add(label, value);
	}
}
