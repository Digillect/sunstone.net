namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1MetaLabelSelector
{
	public V1MetaLabelSelector In(string key, params string[] values)
	{
		var requirement = new V1MetaLabelSelectorRequirement {
			Key = key,
			Operator = "In"
		};

		requirement.Values.Add(values);

		MatchExpressions.Add(requirement);

		return this;
	}

	public V1MetaLabelSelector NotIn(string key, params string[] values)
	{
		var requirement = new V1MetaLabelSelectorRequirement {
			Key = key,
			Operator = "NotIn"
		};

		requirement.Values.Add(values);

		MatchExpressions.Add(requirement);

		return this;
	}

	public V1MetaLabelSelector Exists(string key)
	{
		var requirement = new V1MetaLabelSelectorRequirement {
			Key = key,
			Operator = "Exists"
		};

		MatchExpressions.Add(requirement);

		return this;
	}

	public V1MetaLabelSelector DoesNotExists(string key)
	{
		var requirement = new V1MetaLabelSelectorRequirement {
			Key = key,
			Operator = "DoesNotExists"
		};

		MatchExpressions.Add(requirement);

		return this;
	}

	public V1MetaLabelSelector Equals(string label, string value)
	{
		MatchLabels.Add(label, value);

		return this;
	}
}
