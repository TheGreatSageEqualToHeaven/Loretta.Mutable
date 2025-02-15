using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Mutable.SourceGenerator;

internal sealed class SourceTextReader(SourceText sourceText) : TextReader
{
    private int _position;

    public override int Peek()
    {
        if (_position == sourceText.Length)
        {
            return -1;
        }

        return sourceText[_position];
    }

    public override int Read()
    {
        if (_position == sourceText.Length)
        {
            return -1;
        }

        return sourceText[_position++];
    }

    public override int Read(char[] buffer, int index, int count)
    {
        var charsToCopy = Math.Min(count, sourceText.Length - _position);
        sourceText.CopyTo(_position, buffer, index, charsToCopy);
        _position += charsToCopy;
        return charsToCopy;
    }
}