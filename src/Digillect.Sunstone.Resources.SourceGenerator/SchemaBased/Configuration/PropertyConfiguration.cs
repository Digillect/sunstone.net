using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class PropertyConfiguration
{
	public static readonly PropertyConfiguration Default = new();

	public bool Skip { get; set; }
	public bool? Required { get; set; }
	public PropertyTypeConfiguration? Type { get; set; }
}

public sealed class PropertyConfigurationValidator : AbstractValidator<PropertyConfiguration>
{
	public PropertyConfigurationValidator()
	{
		RuleFor(e => e.Type).SetValidator(new PropertyTypeConfigurationValidator()!).When(e => e is not null);
	}
}
