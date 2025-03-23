using System.Reflection;
using Digillect.Sunstone.Serialization;

namespace Digillect.Sunstone;

public static class ContextExtensions
{
	public static Context<TValues> AddComponentsFromAssembly<TValues>(this Context<TValues> context, Assembly? assembly = null)
		where TValues : notnull
	{
		assembly ??= Assembly.GetCallingAssembly();

		var componentTypes = from type in assembly.GetTypes()
							 where !type.IsAbstract && type.IsAssignableTo(typeof(IComponent<TValues>))
							 select type;

		foreach (var type in componentTypes)
		{
			if (Activator.CreateInstance(type) is not IComponent<TValues> component)
			{
				throw new Exception($"Unable to create component of type {type.Name}");
			}

			component.AddResources(context);
		}

		return context;
	}

	public static Context<TValues> SerializeToJson<TValues>(this Context<TValues> context, string basePath)
		where TValues : class
	{
		context.Resources.Serialize<JsonPersistedObjectSerializer>(basePath);

		return context;
	}

	public static Context<TValues> SerializeToYaml<TValues>(this Context<TValues> context, string basePath)
		where TValues : class
	{
		context.Resources.Serialize<YamlPersistedObjectSerializer>(basePath);

		return context;
	}
}
