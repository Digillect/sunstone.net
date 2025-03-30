namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1BatchJob : IWorkload
{
	public V1CorePodTemplateSpec PodTemplate => Spec.Template;
}
