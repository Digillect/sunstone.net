namespace Digillect.Sunstone.Resources.Kubernetes;

public partial class V1CoreResourceRequirements
{
	public string? MemoryLimit
	{
		get => GetRequirement(_limits, "memory");
		set => SetRequirement(ref _limits, "memory", value);
	}

	public string? CpuLimit
	{
		get => GetRequirement(_limits, "cpu");
		set => SetRequirement(ref _limits, "cpu", value);
	}

	public string? MemoryRequest
	{
		get => GetRequirement(_requests, "memory");
		set => SetRequirement(ref _requests, "memory", value);
	}

	public string? CpuRequest
	{
		get => GetRequirement(_requests, "cpu");
		set => SetRequirement(ref _requests, "cpu", value);
	}

	private string? GetRequirement(Dictionary<string, string>? dictionary, string key)
	{
		if (dictionary is null)
		{
			return null;
		}

		return !dictionary.TryGetValue(key, out string? value) ? null : value;
	}

	private void SetRequirement(ref Dictionary<string, string>? dictionary, string key, string? value)
	{
		dictionary ??= CreateAndAdoptDictionary<string>();

		if (value is null)
		{
			dictionary.Remove(key);
		}
		else
		{
			dictionary[key] = value;
		}
	}
}
