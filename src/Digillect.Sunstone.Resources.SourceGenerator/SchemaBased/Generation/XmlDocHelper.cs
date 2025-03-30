using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace Digillect.Sunstone.Resources.SourceGenerator.SchemaBased.Generation;

public static class XmlDocHelper
{
	private static readonly Regex Backticks = new("`(.+)`");

	public static void WriteDocComment(IndentedTextWriter writer, string description, string? kubernetesName = null, string? clrName = null)
	{
		if (string.IsNullOrEmpty(description))
		{
			return;
		}

		string[] parts = description.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

		for (int i = 0; i < parts.Length; ++i)
		{
			string tag = i == 0 ? "summary" : "para";
			string text = Backticks.Replace(parts[i], match => $"<c>{match.Groups[1].Value}</c>");

			if (kubernetesName is not null && clrName is not null)
			{
				text = text.Replace(kubernetesName, clrName);
			}

			writer.WriteLine("/// <{0}>", tag);
			writer.WriteLine("/// {0}", text);
			writer.WriteLine("/// </{0}>", tag);
		}
	}
}
