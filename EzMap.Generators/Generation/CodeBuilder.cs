using System.Text;

namespace EzMap.Generators.Generation;

/// <summary>
/// Helper class for building C# source code.
/// </summary>
internal class CodeBuilder
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel = 0;
    private const string IndentString = "    ";

    public void AppendLine(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            _builder.AppendLine();
        }
        else
        {
            for (int i = 0; i < _indentLevel; i++)
                _builder.Append(IndentString);
            _builder.AppendLine(line);
        }
    }

    public void Append(string text)
    {
        _builder.Append(text);
    }

    public void IncreaseIndent()
    {
        _indentLevel++;
    }

    public void DecreaseIndent()
    {
        if (_indentLevel > 0)
            _indentLevel--;
    }

    public void AppendOpenBrace()
    {
        AppendLine("{");
        IncreaseIndent();
    }

    public void AppendCloseBrace()
    {
        DecreaseIndent();
        AppendLine("}");
    }

    public override string ToString()
    {
        return _builder.ToString();
    }

    public void Clear()
    {
        _builder.Clear();
        _indentLevel = 0;
    }
}
