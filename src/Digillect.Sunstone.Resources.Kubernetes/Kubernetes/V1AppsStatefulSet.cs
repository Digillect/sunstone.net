namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1AppsStatefulSet : IWorkload
{
	public V1CorePodTemplateSpec PodTemplate => Spec.Template;
}