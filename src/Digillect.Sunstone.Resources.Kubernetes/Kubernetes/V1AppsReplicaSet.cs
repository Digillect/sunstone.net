namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1AppsReplicaSet : IWorkload
{
	public V1CorePodTemplateSpec PodTemplate => Spec.Template;
}