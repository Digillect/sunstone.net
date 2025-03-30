using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class GeneratorConfiguration
{
	public IReadOnlyCollection<string> OpenApiFiles { get; set; } = [];
	public IReadOnlyCollection<string> CustomResourceDefinitions { get; set; } = [];
	public string Namespace { get; set; } = default!;
    public IReadOnlyList<string> TopLevelResources { get; set; } = [];
    public IDictionary<string, ResourceConfiguration> Resources { get; set; } = new Dictionary<string, ResourceConfiguration>();
}

public sealed class GeneratorConfigurationValidator : AbstractValidator<GeneratorConfiguration>
{
	public GeneratorConfigurationValidator()
	{
		RuleForEach(e => e.OpenApiFiles).NotEmpty();
		RuleForEach(e => e.CustomResourceDefinitions).NotEmpty();

		RuleFor(e => e.Namespace).IsValidClassName();

		RuleForEach(e => e.TopLevelResources).NotEmpty();

		RuleForEach(e => e.Resources).SetValidator(new ResourceMappingValidator());
	}
}

public sealed class ResourceMappingValidator : AbstractValidator<KeyValuePair<string, ResourceConfiguration>>
{
	public ResourceMappingValidator()
	{
		RuleFor(e => e.Key).NotEmpty();
		RuleFor(e => e.Value).SetValidator(new ResourceConfigurationValidator());
	}
}
