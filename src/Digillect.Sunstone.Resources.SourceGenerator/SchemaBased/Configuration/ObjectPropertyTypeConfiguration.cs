using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class ObjectPropertyTypeConfiguration
{
	public string ClrType { get; set; } = default!;
	public string? AdditionalSchemaId { get; set; }
}

public sealed class ObjectPropertyTypeConfigurationValidator : AbstractValidator<ObjectPropertyTypeConfiguration>
{
	public ObjectPropertyTypeConfigurationValidator()
	{
		RuleFor(e => e.ClrType).IsValidClassName();
		RuleFor(e => e.AdditionalSchemaId).NotEmpty().When(e => e is not null);
	}
}
