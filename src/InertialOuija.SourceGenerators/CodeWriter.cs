using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace InertialOuija.SourceGenerators;
internal class CodeWriter : IndentedTextWriter
{
	private readonly MemoryStream _memory;

	public int Indentation => base.Indent;

	private CodeWriter(MemoryStream stream, string tabString) : base(new StreamWriter(stream, Encoding.UTF8), tabString)
	{
		_memory = stream;
	}

	public CodeWriter(string tabString = "\t") : this(new MemoryStream(), tabString)
	{ }

	public new CodeWriter WriteLine(string line)
	{
		base.WriteLine(line);
		return this;
	}

	public CodeWriter WriteLine(string line, bool condition)
	{
		if (condition)
			WriteLine(line);
		return this;
	}

	public new CodeWriter Write(string line)
	{
		base.Write(line);
		return this;
	}

	public CodeWriter Write(string line, bool condition)
	{
		if (condition)
			Write(line);
		return this;
	}

	public new CodeWriter Indent()
	{
		base.Indent++;
		return this;
	}

	public CodeWriter Unindent()
	{
		base.Indent--;
		return this;
	}

	public CodeWriter OpenBrace()
	{
		base.WriteLine('{');
		return Indent();
	}

	public CodeWriter CloseBrace()
	{
		Unindent();
		base.WriteLine('}');
		return this;
	}


	public SourceText ToSourceText()
	{
		Flush();
		_memory.Position = 0;
		var sourceText = SourceText.From(_memory, Encoding.UTF8, canBeEmbedded: true);
		_memory.Seek(0, SeekOrigin.End);
		return sourceText;
	}


	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_memory.Dispose();
	}
}
