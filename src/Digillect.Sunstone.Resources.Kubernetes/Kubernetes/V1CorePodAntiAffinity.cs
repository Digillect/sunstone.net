namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CorePodAntiAffinity
{
	public V1CoreWeightedPodAffinityTerm AddPreferred(Action<V1CoreWeightedPodAffinityTerm>? configure = null)
	{
		var term = new V1CoreWeightedPodAffinityTerm();

		configure?.Invoke(term);

		PreferredDuringSchedulingIgnoredDuringExecution.Add(term);

		return term;
	}
}
