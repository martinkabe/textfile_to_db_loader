using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public enum SepType
{
    auto, comma, tilda, tab, space, dot, dash, semiColon, pipe, caret
}

class AutoMethods
{
    public static char AutoDetectCsvSeparator(StreamReader reader, int rowCount, IList<char> separators)
    {
        IList<int> separatorsCount = new int[separators.Count];

        int character;

        int row = 0;

        bool quoted = false;
        bool firstChar = true;

        while (row < rowCount)
        {
            character = reader.Read();

            switch (character)
            {
                case '"':
                    if (quoted)
                    {
                        if (reader.Peek() != '"') // Value is quoted and current character is " and next character is not ".
                            quoted = false;
                        else
                            reader.Read(); // Value is quoted and current and next characters are "" - read (skip) peeked qoute.
                    }
                    else
                    {
                        if (firstChar) // Set value as quoted only if this quote is the first char in the value.
                            quoted = true;
                    }
                    break;
                case '\n':
                    if (!quoted)
                    {
                        ++row;
                        firstChar = true;
                        continue;
                    }
                    break;
                case -1:
                    row = rowCount;
                    break;
                default:
                    if (!quoted)
                    {
                        int index = separators.IndexOf((char)character);
                        if (index != -1)
                        {
                            ++separatorsCount[index];
                            firstChar = true;
                            continue;
                        }
                    }
                    break;
            }

            if (firstChar)
                firstChar = false;
        }

        int maxCount = separatorsCount.Max();

        return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
    }

    public static char DetectSeparator(string pathToTextFile, SepType sepType = SepType.auto, int lookupRows = 250000)
    {
        char sep = '\0';
        switch (sepType)
        {
            case SepType.auto:
                using (StreamReader sr_sep = new StreamReader(pathToTextFile))
                {
                    IList<char> seps = new List<char>() { '\t', ',', '.', ';', '~', ' ', '^', '-', '|' };
                    sep = Convert.ToChar(AutoDetectCsvSeparator(sr_sep, lookupRows, seps).ToString());
                }
                break;
            case SepType.comma:
                sep = ',';
                break;
            case SepType.tilda:
                sep = '~';
                break;
            case SepType.tab:
                sep = '\t';
                break;
            case SepType.space:
                sep = ' ';
                break;
            case SepType.dot:
                sep = '.';
                break;
            case SepType.dash:
                sep = '-';
                break;
            case SepType.semiColon:
                sep = ';';
                break;
            case SepType.pipe:
                sep = '|';
                break;
            case SepType.caret:
                sep = '^';
                break;
            default:
                throw (new ArgumentException("Invalid separator!"));
        }
        return sep;
    }

    public static char SetSeparator(SepType sepType = SepType.auto)
    {
        char sep = '\0';
        switch (sepType)
        {
            case SepType.auto:
                sep = ',';
                break;
            case SepType.comma:
                sep = ',';
                break;
            case SepType.tilda:
                sep = '~';
                break;
            case SepType.tab:
                sep = '\t';
                break;
            case SepType.space:
                sep = ' ';
                break;
            case SepType.dot:
                sep = '.';
                break;
            case SepType.dash:
                sep = '-';
                break;
            case SepType.semiColon:
                sep = ';';
                break;
            case SepType.pipe:
                sep = '|';
                break;
            case SepType.caret:
                sep = '^';
                break;
            default:
                throw (new ArgumentException("Invalid separator!"));
        }
        return sep;
    }
}
