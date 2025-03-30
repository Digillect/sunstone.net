namespace Digillect.Sunstone.Resources.Kubernetes;

public class EnvironmentHelper(SunstoneObject owner, BaseArray<V1CoreEnvVar> env, BaseArray<V1CoreEnvFromSource> envFrom)
{
	private string? GetRootName()
	{
		SunstoneObject owner1 = owner;

		while (true)
		{
			var parent = owner1.GetParent();

			if (parent is null)
			{
				break;
			}

			owner1 = parent;
		}

		if (owner1 is IHaveMetadata objectWithMetadata)
		{
			return objectWithMetadata.Metadata.Name;
		}

		return null;
	}

	public EnvironmentHelper Add(string name, string value)
	{
		var envVar = new V1CoreEnvVar {
			Name = name,
			Value = value
		};

		AddOrReplaceEnvVar(envVar);

		return this;
	}

	public EnvironmentHelper AddConfigMapKey(string name, string? configMapName = null, string? key = null, bool? optional = null)
	{
		var envVar = new V1CoreEnvVar {
			Name = name,
			ValueFrom = {
				ConfigMapKeyRef = {
					Key = key ?? name,
					Name = configMapName ?? GetRootName()
				}
			}
		};

		if (optional is not null)
		{
			envVar.ValueFrom.ConfigMapKeyRef.Optional = optional;
		}

		AddOrReplaceEnvVar(envVar);

		return this;
	}

	public EnvironmentHelper AddSecretKey(string name, string? secretName = null, string? key = null, bool? optional = null)
	{
		var envVar = new V1CoreEnvVar {
			Name = name,
			ValueFrom = {
				SecretKeyRef = {
					Key = key ?? name,
					Name = secretName ?? GetRootName()
				}
			}
		};

		if (optional is not null)
		{
			envVar.ValueFrom.ConfigMapKeyRef.Optional = optional;
		}

		AddOrReplaceEnvVar(envVar);

		return this;
	}

	public EnvironmentHelper AddField(string name, string fieldPath)
	{
		var envVar = new V1CoreEnvVar {
			Name = name,
			ValueFrom = {
				FieldRef = {
					FieldPath = fieldPath,
				}
			}
		};

		AddOrReplaceEnvVar(envVar);

		return this;
	}

	public EnvironmentHelper AddResourceField(string name, string resource, string? containerName = null, string? divisor = null)
	{
		var envVar = new V1CoreEnvVar {
			Name = name,
			ValueFrom = {
				ResourceFieldRef = {
					Resource = resource,
					ContainerName = containerName,
					Divisor = divisor
				}
			}
		};

		AddOrReplaceEnvVar(envVar);

		return this;
	}

	public EnvironmentHelper UseConfigMap(string? name = null, bool optional = false, string? prefix = null)
	{
		var envFromSource = new V1CoreEnvFromSource {
			ConfigMapRef = {
				Name = name ?? GetOwnerName()
			},
			Prefix = prefix
		};

		if (optional)
		{
			envFromSource.ConfigMapRef.Optional = true;
		}

		envFrom.Add(envFromSource);

		return this;
	}

	public EnvironmentHelper UseSecret(string? name = null, bool optional = false, string? prefix = null)
	{
		var envFromSource = new V1CoreEnvFromSource {
			SecretRef = {
				Name = name ?? GetOwnerName()
			},
			Prefix = prefix
		};

		if (optional)
		{
			envFromSource.SecretRef.Optional = true;
		}

		envFrom.Add(envFromSource);

		return this;
	}

	private void AddOrReplaceEnvVar(V1CoreEnvVar envVar)
	{
		env.AddOrReplace(envVar, e => e.Name == envVar.Name);
	}

	private string GetOwnerName()
	{
		for (var item = owner; item is not null; item = item.GetParent())
		{
			if (item is IHaveMetadata resourceWithMetadata)
			{
				if (!string.IsNullOrEmpty(resourceWithMetadata.Metadata.Name))
				{
					return resourceWithMetadata.Metadata.Name;
				}

				break;
			}
		}

		throw new InvalidOperationException("Resource name must be specified");
	}
}
