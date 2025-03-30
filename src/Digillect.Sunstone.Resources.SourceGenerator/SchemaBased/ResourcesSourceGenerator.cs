using System.Collections.Immutable;
using System.Text.Json;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Configuration;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;
using Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased;

[Generator]
public class ResourcesSourceGenerator : IIncrementalGenerator
{
	private static readonly DiagnosticDescriptor ConfigurationNotProvided = new(
		"SS0001",
		"Generator configuration file generator-config.json does not contain any configuration",
		"Generator configuration file generator-config.json does not contain any configuration",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	private static readonly DiagnosticDescriptor ConfigurationParsingError = new(
		"SS0002",
		"Error parsing configuration file",
		"Error parsing configuration file: {0}",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	private static readonly DiagnosticDescriptor ConfigurationValidationError = new(
		"SS0003",
		"Invalid configuration",
		"Generator configuration file validation failed: {0}",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	private static readonly DiagnosticDescriptor OpenApiFileNotFoundError = new(
		"SS0010",
		"OpenAPI file is not found",
		"OpenAPI file {0} is not found",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	private static readonly DiagnosticDescriptor OpenApiFileIsEmptyError = new(
		"SS0011",
		"OpenAPI file is empty",
		"OpenAPI file {0} is empty",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	private static readonly DiagnosticDescriptor OpenApiFileParsingError = new(
		"SS0012",
		"Error parsing OpenAPI document",
		"Error parsing OpenAPI document {0}: {1}",
		"Generator",
		DiagnosticSeverity.Error,
		true);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var configProvider = context.AdditionalTextsProvider
			.Where(text => text.Path.EndsWith("generator-config.json"));

		var jsonFiles = context.AdditionalTextsProvider
			.Where(text => text.Path.EndsWith(".json") && !text.Path.EndsWith("generator-config.json"))
			.Collect();

		var yamlFiles = context.AdditionalTextsProvider
			.Where(text => text.Path.EndsWith(".yaml") || text.Path.EndsWith(".yml"))
			.Collect();

		var combined = configProvider.Combine(jsonFiles).Combine(yamlFiles);

		// Generate the source code.
		context.RegisterSourceOutput(combined,
			((ctx, t) => GenerateCode(ctx, t.Left.Left, t.Left.Right, t.Right)));
	}

	private void GenerateCode(
		SourceProductionContext ctx,
		AdditionalText configText,
		ImmutableArray<AdditionalText> jsonFiles,
		ImmutableArray<AdditionalText> yamlFiles)
	{
		var result = LoadConfiguration(ctx, configText);
		if (result.IsError)
		{
			return;
		}

		var config = result.Value;

		foreach (string openApiFileName in config.OpenApiFiles)
		{
			GenerateFromOpenApi(ctx, config, openApiFileName, jsonFiles);
		}

		foreach (string crd in config.CustomResourceDefinitions)
		{
			GenerateFromCrd(ctx, config, crd, yamlFiles);
		}

		ctx.AddSource("ResourcesCollectionExtensions.cs", OpenApiResourcesGenerator.GenerateResourcesFactory(config));
	}

	private void GenerateFromCrd(SourceProductionContext ctx, GeneratorConfiguration config, string crdFileName, ImmutableArray<AdditionalText> yamlFiles)
	{
		var crdFile = yamlFiles.FirstOrDefault(file => file.Path.EndsWith(crdFileName));
		if (crdFile is null)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileNotFoundError, null, crdFileName));
			return;
		}

		string sourceText = crdFile.GetText(ctx.CancellationToken)?.ToString() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(sourceText))
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileIsEmptyError, null, crdFileName));
		}

		var result = CrdParser.ParseCrd(sourceText);
		if (result.IsError)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileParsingError, null, crdFileName, result.Error.ToString()));
			return;
		}

		foreach (var kvp in OpenApiResourcesGenerator.GenerateResources(result.Value, config))
		{
			ctx.AddSource($"{kvp.Key.TypeName}.cs", kvp.Value);
		}
	}

	private void GenerateFromOpenApi(SourceProductionContext ctx, GeneratorConfiguration config, string openApiFileName, ImmutableArray<AdditionalText> jsonFiles)
	{
		var openApiFile = jsonFiles.FirstOrDefault(file => file.Path.EndsWith(openApiFileName));
		if (openApiFile is null)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileNotFoundError, null, openApiFileName));
			return;
		}

		string sourceText = openApiFile.GetText(ctx.CancellationToken)?.ToString() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(sourceText))
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileIsEmptyError, null, openApiFileName));
			return;
		}

		var result = OpenApiParser.ParseOpenApiFile(sourceText);
		if (result.IsError)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(OpenApiFileParsingError, null, openApiFileName, result.Error.ToString()));
			return;
		}

		foreach (var kvp in OpenApiResourcesGenerator.GenerateResources(result.Value, config))
		{
			ctx.AddSource($"{kvp.Key.TypeName}.cs", kvp.Value);
		}
	}

	private Result<GeneratorConfiguration> LoadConfiguration(SourceProductionContext ctx, AdditionalText configText)
	{
		var sourceText = configText.GetText(ctx.CancellationToken);
		string? text = sourceText?.ToString();

		if (sourceText is null || string.IsNullOrWhiteSpace(text))
		{
			ctx.ReportDiagnostic(Diagnostic.Create(ConfigurationNotProvided, null));
			return Error.Generic("Configuration file is empty or does not exist.");
		}

		GeneratorConfiguration config;

		try
		{
			config = JsonSerializer.Deserialize<GeneratorConfiguration>(text!)!;
		}
		catch (JsonException ex)
		{
			Location? location = null;

			if (ex.LineNumber is not null)
			{
				if (ex.LineNumber < sourceText.Lines.Count)
				{
					var span = sourceText.Lines[(int) ex.LineNumber].Span;
					LinePosition startLinePosition = LinePosition.Zero;
					LinePosition endLinePosition = LinePosition.Zero;

					if (ex.BytePositionInLine is not null)
					{
						span = TextSpan.FromBounds(span.Start + (int) ex.BytePositionInLine, span.End);
						startLinePosition = new LinePosition((int) ex.LineNumber, (int) ex.BytePositionInLine);
						endLinePosition = new LinePosition(startLinePosition.Line, startLinePosition.Character);
					}

					location = Location.Create(configText.Path, span, new LinePositionSpan(startLinePosition, endLinePosition));
				}
			}

			ctx.ReportDiagnostic(Diagnostic.Create(ConfigurationParsingError, location, ex.Message));
			return Error.Generic("Error parsing configuration file.");
		}
		catch (Exception ex)
		{
			ctx.ReportDiagnostic(Diagnostic.Create(ConfigurationParsingError, null, ex.Message));
			return new Exceptional(ex);
		}

		var validator = new GeneratorConfigurationValidator();
		var result = validator.Validate(config!);

		if (!result.IsValid)
		{
			foreach (var error in result.Errors)
			{
				ctx.ReportDiagnostic(Diagnostic.Create(ConfigurationValidationError, null, error.ErrorMessage));
			}

			return Error.Generic("Configuration validation failed");
		}

		return config;
	}
}
