using System.CodeDom.Compiler;

namespace Digillect.Sunstone.Resources.SourceGenerator.Generation;

public static class IndentedTextWriterExtensions
{
	public static void WriteEmptyLine(this IndentedTextWriter writer)
	{
		writer.WriteLineNoTabs(string.Empty);
	}

	public static void WriteBlock(this IndentedTextWriter writer, Action? action = null, bool withSemicolon = false)
	{
		writer.WriteLine("{");

		if (action is not null)
		{
			writer.Indent++;
			action();
			writer.Indent--;
		}

		writer.WriteLine(withSemicolon ? "};" : "}");
	}

	public static void WriteIndented(this IndentedTextWriter writer, Action action)
	{
		writer.Indent++;
		action();
		writer.Indent--;
	}

	public static bool WriteOneLinePerItem<T>(this IndentedTextWriter writer, IEnumerable<T> items, Func<T, string> lineGenerator, bool addEmptyLine = true)
	{
		bool generatedSomething = false;

		foreach (var item in items)
		{
			writer.WriteLine(lineGenerator(item));

			generatedSomething = true;
		}

		if (addEmptyLine)
		{
			writer.WriteEmptyLine();
		}

		return generatedSomething;
	}

	public static bool WriteItems<T>(this IndentedTextWriter writer, IEnumerable<T> items, Action<T> generator, bool separateWithEmptyLine = true)
	{
		bool generatedSomething = false;

		foreach (var item in items)
		{
			if (generatedSomething && separateWithEmptyLine)
			{
				writer.WriteEmptyLine();
			}

			generator(item);

			generatedSomething = true;
		}

		return generatedSomething;
	}
}
