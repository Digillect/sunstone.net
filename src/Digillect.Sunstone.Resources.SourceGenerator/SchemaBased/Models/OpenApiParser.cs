using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Models;

public static class OpenApiParser
{
    public static Result<OpenApiDocument> ParseOpenApiFile(string sourceText)
    {
        using var textReader = new StringReader(sourceText);
        var reader = new OpenApiTextReaderReader();

        OpenApiDocument document;

        try
        {
            document = reader.Read(textReader, out _);
        }
        catch (Exception ex)
        {
            return Error.Generic(ex.Message);
        }

        return document;
    }
}
