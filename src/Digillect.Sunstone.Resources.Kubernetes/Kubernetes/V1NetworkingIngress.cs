namespace Digillect.Sunstone.Resources.Kubernetes;

public enum HTTPIngressPathType
{
	ImplementationSpecific = 0,
	Exact,
	Prefix
}

public partial class V1NetworkingHTTPIngressRuleValue
{
	public V1NetworkingHTTPIngressPath AddPath(Action<V1NetworkingHTTPIngressPath>? configure = null)
	{
		var path = new V1NetworkingHTTPIngressPath();

		Paths.Add(path);

		configure?.Invoke(path);

		return path;
	}
}

public partial class V1NetworkingIngressRule
{
	public V1NetworkingHTTPIngressPath AddServicePath(
		string serviceName,
		NameOrPort servicePort,
		string? path = null,
		HTTPIngressPathType pathType = HTTPIngressPathType.ImplementationSpecific)
	{
		return Http.AddPath(p => {
			p.Backend.Service.Name = serviceName;

			if (servicePort.Port is not null)
			{
				p.Backend.Service.Port.Number = servicePort.Port;
			}
			else
			{
				p.Backend.Service.Port.Name = servicePort.Name;
			}

			p.Path = path;
			p.PathType = pathType;
		});
	}
}
public partial class V1NetworkingIngress
{
	public V1NetworkingIngressRule AddServiceRule(
		string host,
		string? serviceName = null,
		string? path = null,
		HTTPIngressPathType pathType = HTTPIngressPathType.ImplementationSpecific,
		NameOrPort? port = null,
		Action<V1NetworkingIngressRule>? configureRule = null)
	{
		serviceName ??= Metadata.Name;

		ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

		return AddIngressRule(host, rule => {
			rule.AddServicePath(serviceName, port ?? NameOrPort.Http, path, pathType);

			configureRule?.Invoke(rule);
		});
	}

	public V1NetworkingIngressRule AddIngressRule(string host, Action<V1NetworkingIngressRule>? configure)
	{
		var rule = new V1NetworkingIngressRule { Host = host };

		configure?.Invoke(rule);

		Spec.Rules.Add(rule);

		return rule;
	}

	public V1NetworkingIngressTLS AddTLS(string secretName, params string[] hosts)
	{
		var tls = new V1NetworkingIngressTLS { SecretName = secretName };

		if (hosts.Length > 0)
		{
			foreach (string host in hosts)
			{
				tls.Hosts.Add(host);
			}
		}

		Spec.Tls.Add(tls);

		return tls;
	}
}
