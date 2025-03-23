using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class ScalarPropertyTypeConfiguration
{
	public string ClrType { get; set; } = default!;
}

public sealed class ScalarPropertyTypeConfigurationValidator : AbstractValidator<ScalarPropertyTypeConfiguration>
{
	public ScalarPropertyTypeConfigurationValidator()
	{
		RuleFor(e => e.ClrType).NotEmpty();
	}
}
