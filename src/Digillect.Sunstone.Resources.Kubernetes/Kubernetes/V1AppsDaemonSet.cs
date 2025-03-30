namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1AppsDaemonSet : IWorkload
{
	public V1CorePodTemplateSpec PodTemplate => Spec.Template;
}
