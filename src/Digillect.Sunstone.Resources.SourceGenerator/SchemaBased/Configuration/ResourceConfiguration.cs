using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class ResourceConfiguration
{
	public static readonly ResourceConfiguration Default = new();

	public string BaseClass { get; set; } = "Digillect.Sunstone.SunstoneObject";
	public IReadOnlyList<string> AdditionalInterfaces { get; set; } = [];
	public IDictionary<string, PropertyConfiguration> Properties { get; set; } = new Dictionary<string, PropertyConfiguration>();
	public IReadOnlyList<string> IgnoredProperties { get; set; } = [];
	public IReadOnlyList<string> PropertiesPriority { get; set; } = [];

	public bool ShouldIgnoreProperty(string name)
	{
		return Properties.TryGetValue(name, out var propertyConfiguration)
			? propertyConfiguration.Skip
			: IgnoredProperties.Contains(name);
	}
}

public sealed class ResourceConfigurationValidator : AbstractValidator<ResourceConfiguration>
{
	public ResourceConfigurationValidator()
	{
		RuleFor(e => e.BaseClass).NotEmpty();
		RuleForEach(e => e.Properties).SetValidator(new PropertyMappingValidator());
		RuleForEach(e => e.IgnoredProperties).NotEmpty();
		RuleForEach(e => e.PropertiesPriority).NotEmpty();
	}
}

public sealed class PropertyMappingValidator : AbstractValidator<KeyValuePair<string, PropertyConfiguration>>
{
	public PropertyMappingValidator()
	{
		RuleFor(e => e.Key).NotEmpty();
		RuleFor(e => e.Value).SetValidator(new PropertyConfigurationValidator());
	}
}
