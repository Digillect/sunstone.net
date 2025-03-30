namespace Digillect.Sunstone.Resources.Kubernetes;

public interface ICanMatchLabels
{
	void MatchLabel(string label, string value);
}
