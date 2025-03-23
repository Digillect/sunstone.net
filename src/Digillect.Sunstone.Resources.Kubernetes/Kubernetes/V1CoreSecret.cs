namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CoreSecret
{
	protected override string ConvertValue(string value)
	{
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
	}
}