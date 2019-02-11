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
                var tokenizer = StringTokenizer.Tokenize(str, commentSymbol);
                foreach (var token in tokenizer)
                {
                    // don't split if the token only contains an ending comma
                    if (token.IndexOf(',') == token.Length - 1)
                    {
                        yield return token.Substring(0, token.Length - 1);
                        continue;
                    }

                    var newToken = token;

                    // remove the ending comma (if the token contains a separator comma AND and ending comma)
                    if (newToken.EndsWith(','))
                    {
                        newToken = newToken.Substring(0, token.Length - 1);
                    }

                    foreach (var entry in newToken.Split(','))
                    {
                        yield return entry;
                    }
                }

            }

            return new CommaSeparatedTokenizer(TokenizeInternal());
        }

    }
}
