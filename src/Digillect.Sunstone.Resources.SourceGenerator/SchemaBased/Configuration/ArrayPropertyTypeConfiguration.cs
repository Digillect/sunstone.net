using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class ArrayPropertyTypeConfiguration
{
	public PropertyTypeConfiguration ItemType { get; set; } = default!;
}

public sealed class ArrayPropertyTypeConfigurationValidator : AbstractValidator<ArrayPropertyTypeConfiguration>
{
	public ArrayPropertyTypeConfigurationValidator(PropertyTypeConfigurationValidator propertyTypeConfigurationValidator)
	{
		RuleFor(e => e.ItemType).SetValidator(propertyTypeConfigurationValidator);
	}
}
