using System.Collections.Generic;

namespace HSNXT.Common
{
    /// <summary>
    /// A string tokenizer that splits tokens by a comma.
    /// </summary>
    public class CommaSeparatedTokenizer : EnumeratorTokenizer
    {
        public CommaSeparatedTokenizer(IEnumerable<string> provider) : base(provider)
        {
        }

        public static CommaSeparatedTokenizer Tokenize(string str, char commentSymbol = '#')
        {
            IEnumerable<string> TokenizeInternal()
            {
                var tokenizer = StringTokenizer.Tokenize(str);

                foreach (var token in tokenizer)
                {
                    // don't split if the token only contains a comma with nothing after it
                    if (token[token.Length - 1] == ',') continue;
                    
                    foreach (var entry in token.Split(','))
                    {
                        yield return entry;
                    }
                }
            }

            return new CommaSeparatedTokenizer(TokenizeInternal());
        }

    }
}
