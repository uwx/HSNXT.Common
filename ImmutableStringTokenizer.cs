// Extracted from a version of RayBot (C# rewrite) at April 15th, 2019.

namespace RayBotSharp.NeoCommandSystem
{
    public readonly struct ImmutableStringTokenizer
    {
        internal readonly string SourceString;
        internal readonly int StartIndex;

        public ImmutableStringTokenizer(string sourceString, int startIndex = 0)
        {
            SourceString = sourceString;
            StartIndex = startIndex;
        }
    }

    public static class ImmutableStringTokenizerExtensions
    {
        private static readonly char[] WhiteSpaceChars =
        {
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            ' ',
            '\u00a0',
            '\u0085',
        };
        
        // no point having aggressive inlining any of these methods since GetNext is already too big for that
        private static string Slice(string str, int from, int to)
            => str.Substring(from, (to == -1 ? str.Length : to) - from);

        // closures create massive reference types so don't make this a local function
        private static bool IsValidIndex(string str, int i)
            => i != -1 && i < str.Length;
        
        // extension method to minimize ldfld calls
        public static bool GetNext(
            this ImmutableStringTokenizer self, out ImmutableStringTokenizer nextTokenizer, out string nextToken)
        {
            // minimize struct copies & ldflda calls (you get loads of `mov eax, [ebp-0x1c]` instructions when not doing this)
            var str = self.SourceString;
            var index = self.StartIndex;
            
            if (index == -1 || str == null)
            {
                nextTokenizer = default;
                nextToken = default;
                return false;
            }
            
            // ReSharper disable once InvertIf
            while (char.IsWhiteSpace(str[index]))
            {
                index++;

                if (!IsValidIndex(str, index))
                {
                    nextTokenizer = default;
                    nextToken = default;
                    return false;
                }
            }
            
            // support quoted text
            var newIndex = str[index] == '"'
                // go to end of quote (++index to remove initial quote)
                ? str.IndexOf('"', ++index)
                // not quoted, go to next whitespace
                : str.IndexOfAny(WhiteSpaceChars, index);
            
            nextToken = Slice(str, index, newIndex);

            // remove closing quote (increment once if the current character is a quote, not whitespace)
            if (IsValidIndex(str, newIndex) && str[newIndex] == '"') newIndex++;
            
            nextTokenizer = new ImmutableStringTokenizer(str, newIndex);
            return true;
        }
    }
}
