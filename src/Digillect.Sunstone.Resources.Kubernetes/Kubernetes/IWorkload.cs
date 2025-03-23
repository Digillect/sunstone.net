namespace Digillect.Sunstone.Resources.Kubernetes;

public interface IWorkload : IHaveMetadata
{
	V1CorePodTemplateSpec PodTemplate { get; }
}

public static class WorkloadExtensions
{
	public static T PodSpec<T>(this T workload, Action<V1CorePodSpec> configure)
		where T : IWorkload
	{
		configure(workload.PodTemplate.Spec);

		return workload;
	}

	public static T MainContainer<T>(this T workload, Action<V1CoreContainer> configure)
		where T : IWorkload
	{
		workload.PodTemplate.Spec.MainContainer(configure);

		return workload;
	}

	public static T InitContainer<T>(this T workload, string name, Action<V1CoreContainer> configure)
		where T : IWorkload
	{
		workload.PodTemplate.Spec.InitContainer(name, configure);

		return workload;
	}
}
