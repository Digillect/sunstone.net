using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public sealed class PropertyTypeConfiguration
{
	public ScalarPropertyTypeConfiguration? Scalar { get; set; }
	public ObjectPropertyTypeConfiguration? Object { get; set; }
	public ArrayPropertyTypeConfiguration? Array { get; set; }
	public DictionaryPropertyTypeConfiguration? Dictionary { get; set; }
}

public sealed class PropertyTypeConfigurationValidator : AbstractValidator<PropertyTypeConfiguration>
{
	public PropertyTypeConfigurationValidator()
	{
		RuleFor(x => x.Scalar).SetValidator(new ScalarPropertyTypeConfigurationValidator()!).When(e => e is not null);
		RuleFor(x => x.Object).SetValidator(new ObjectPropertyTypeConfigurationValidator()!).When(e => e is not null);
		RuleFor(x => x.Array).SetValidator(new ArrayPropertyTypeConfigurationValidator(this)!).When(e => e is not null);
		RuleFor(x => x.Dictionary).SetValidator(new DictionaryPropertyTypeConfigurationValidator(this)!).When(e => e is not null);
	}
}
