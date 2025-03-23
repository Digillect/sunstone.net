namespace Digillect.Sunstone.Resources.SourceGenerator.AttributeBased.Semantic;

public interface IResourceClass;

public sealed class RejectedClass : IResourceClass
{
	public static readonly RejectedClass Instance = new();
}

public sealed class ResourceClass : IResourceClass
{
	public string Namespace { get; set; } = null!;
	public string Name { get; set; } = null!;

	public string? ApiVersion { get; set; }
	public string? Kind { get; set; }

	public IReadOnlyList<ResourceProperty> Properties { get; set; } = [];

	public bool IsTopLevelResource => ApiVersion is not null && Kind is not null;

	public bool GenerateCreateMethod { get; set; }
	public bool GeneratePersistMethod { get; set; }
}

public abstract class ResourceProperty
{
	protected ResourceProperty(string name, string kubernetesName, string type, bool isPersisted, int? persistencePriority)
	{
		Name = name;
		KubernetesName = kubernetesName;
		Type = type;
		IsPersisted = isPersisted;
		PersistencePriority = persistencePriority;
	}

	public abstract string PersistenceMember { get; }
	public string Name { get; }
	public string KubernetesName { get; }
	public string Type { get; }
	public bool IsPersisted { get; }
	public int? PersistencePriority { get; }
}

public sealed class ScalarProperty : ResourceProperty
{
	public ScalarProperty(string name, string kubernetesName, string type, bool isPersisted, int? persistencePriority)
		: base(name, kubernetesName, type, isPersisted, persistencePriority)
	{
	}

	public override string PersistenceMember => Name;
}

public abstract class ComplexProperty : ResourceProperty
{
	protected ComplexProperty(string name, string kubernetesName, string type, bool isPersisted, int? persistencePriority, bool generateImplementation)
		: base(name, kubernetesName, type, isPersisted, persistencePriority)
	{
		this.GenerateImplementation = generateImplementation;
	}

	public override string PersistenceMember => $"_{KubernetesName}";
	public bool GenerateImplementation { get; }
}

public sealed class ArrayProperty : ComplexProperty
{
	public ArrayProperty(string name,
		string kubernetesName,
		string type,
		bool isPersisted,
		int? persistencePriority,
		bool generateImplementation,
		string itemType)
		: base(name, kubernetesName, type, isPersisted, persistencePriority, generateImplementation)
	{
		this.ItemType = itemType;
	}

	public string ItemType { get; }
}

public sealed class DictionaryProperty : ComplexProperty
{
	public DictionaryProperty(string name,
		string kubernetesName,
		string type,
		bool isPersisted,
		int? persistencePriority,
		bool generateImplementation,
		string itemType)
		: base(name, kubernetesName, type, isPersisted, persistencePriority, generateImplementation)
	{
		this.ItemType = itemType;
	}

	public string ItemType { get; }
}

public sealed class ObjectProperty : ComplexProperty
{
	public ObjectProperty(string name, string kubernetesName, string type, bool isPersisted, int? persistencePriority, bool generateImplementation)
		: base(name, kubernetesName, type, isPersisted, persistencePriority, generateImplementation)
	{
	}
}
