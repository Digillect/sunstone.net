namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CoreProbe
{
	public V1CoreProbe UseHttp(string path, NameOrPort? port = null)
	{
		HttpGet.Path = path;
		HttpGet.Port = port ?? NameOrPort.Http;

		return this;
	}
}
