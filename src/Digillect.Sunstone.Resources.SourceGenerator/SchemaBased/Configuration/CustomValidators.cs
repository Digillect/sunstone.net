using System.Text.RegularExpressions;
using FluentValidation;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;

public static class CustomValidators
{
	private static readonly Regex CsharpNamespacedClassName= new(@"^([a-zA-Z_][a-zA-Z0-9_\\]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*)\.[a-zA-Z_][a-zA-Z0-9_]*$");

	public static IRuleBuilderOptions<T, string> IsValidClassName<T>(this IRuleBuilder<T, string> ruleBuilder)
	{
		return ruleBuilder
			.Matches(CsharpNamespacedClassName)
			.WithMessage("{PropertyPath} must be a valid C# class name with optional namespace");
	}
}
