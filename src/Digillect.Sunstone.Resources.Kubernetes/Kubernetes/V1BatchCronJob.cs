namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1BatchCronJob : IWorkload
{
	public V1CorePodTemplateSpec PodTemplate => Spec.JobTemplate.Spec.Template;
	
	public V1BatchCronJob Schedule(string schedule)
	{
		Spec.Schedule = schedule;

		return this;
	}
}