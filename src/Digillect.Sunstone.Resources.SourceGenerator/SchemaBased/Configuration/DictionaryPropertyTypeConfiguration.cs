using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class DictionaryPropertyTypeConfiguration
{
	public PropertyTypeConfiguration ValueType { get; set; } = default!;
}

public sealed class DictionaryPropertyTypeConfigurationValidator : AbstractValidator<DictionaryPropertyTypeConfiguration>
{
	public DictionaryPropertyTypeConfigurationValidator(PropertyTypeConfigurationValidator propertyTypeConfigurationValidator)
	{
		RuleFor(x => x.ValueType).SetValidator(propertyTypeConfigurationValidator);
	}
}
