using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSNXT.Common
{
    public interface IStringTokenizer : IEnumerable<string>, IPeekableEnumerator<string>
    {
        /// <summary>
        /// Assigns the next element in the tokenizer to a string
        /// </summary>
        /// <param name="str">the string to assign</param>
        /// <returns>true if successful</returns>
        bool Next(out string str);
        
//        /// <summary>
//        /// Gets all remaining elements in the tokenizer separated by a single space.
//        /// </summary>
//        /// <returns>the remaining elements</returns>
//        string Remaining();
//
//        /// <summary>
//        /// Creates a copy of this tokenizer at the current index, without resetting. 
//        /// </summary>
//        /// <returns>A copy of this tokenizer, or an empty tokenizer if there is nothing to copy</returns>
//        StringTokenizer Clone();
    }

    public abstract class EnumeratorTokenizer : IStringTokenizer
    {
        private readonly IEnumerable<string> _provider;
        private IPeekableEnumerator<string> _with;

        protected EnumeratorTokenizer(IEnumerable<string> provider)
        {
            _provider = provider;
            Reset();
        }
        
        public bool Next(out string str)
        {
            var b = MoveNext();
            str = Current;
            return b;
        }

//        public string Remaining()
//        {
//            var tokens = new StringBuilder();
//            foreach (var token in this)
//            {
//                tokens.Append(' ').Append(token);
//            }
//
//            return tokens.Length == 0 ? "" : tokens.ToString().Substring(1); // remove first space
//        }
//
//        public StringTokenizer Clone()
//        {
//            return !_isMoveNext
//                ? new StringTokenizer(_source, Enumerable.Empty<(int, string)>())
//                : Tokenize(_source.Substring(_with.Current.index));
//        }

        // IEnumerator.Reset (not a delegate implementation)
        public void Reset() => _with = new PeekEnumerator<string>(_provider.GetEnumerator());

        // IEnumerable
        public IEnumerator<string> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        // IPeekableEnumerator
        public string Peek() => _with.Peek();
        public bool TryPeek(out string value) => _with.TryPeek(out value);

        // IEnumerator
        public bool MoveNext() => _with.MoveNext();
        public string Current => _with.Current;
        object IEnumerator.Current => _with.Current;
        public void Dispose() => _with.Dispose();
    }

    /// <summary>
    /// String tokenizer overhaul by uwx
    /// </summary>
    public class StringTokenizer : EnumeratorTokenizer
    {
        public static readonly char[] WhiteSpaceChars =
            new[] { (char) 32, (char) 160, (char) 133 }
                .Concat(Range((char) 9, (char) 13))
                .ToArray();

        private StringTokenizer(IEnumerable<string> provider) : base(provider)
        {
        }

        public static StringTokenizer Tokenize(string str, char commentSymbol = '#')
        {
            IEnumerable<string> TokenizeInternal()
            {
                var index = 0;
                do
                {
                    while (true)
                    {
                        if (str[index] == commentSymbol) // skip to next line on comment
                        {
                            var nextLineIndex = str.IndexOf('\n', index + 1);
                            if (nextLineIndex == -1) // reached end of string
                                yield break;
                            
                            index = nextLineIndex + 1;
                        }
                        else if (char.IsWhiteSpace(str[index]))
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                        if (index >= str.Length || index == -1) yield break;
                    }
                    
                    // support quoted text
                    var newIndex = str[index] == '"'
                        ? str.IndexOf('"', ++index) // ++index to remove initial quote (TODO was +2, likely a bug) 
                        : str.IndexOfAny(WhiteSpaceChars, index); // unquoted, find next whitespace
                    
                    yield return Slice(str, index, newIndex);
                    
                    // remove closing quote (increment once if the current character is a quote, not whitespace)
                    index = newIndex != -1 && newIndex < str.Length && str[newIndex] == '"'
                        ? newIndex + 1
                        : newIndex;
                } while (index != -1);
            }

            return new StringTokenizer(TokenizeInternal());
        }

        private static string Slice(string str, int from, int to)
            => str.Substring(from, (to == -1 ? str.Length : to) - from);

        private static IEnumerable<char> Range(char start, char end)
        {
            for (var i = start; i <= end; i++)
            {
                yield return i;
            }
        }
    }

    public interface IPeekableEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// Gets the next entry in the enumerator, and caches it so the first following invocation of
        /// <see cref="IEnumerator.MoveNext"/> will simply return the cached value.
        /// </summary>
        /// <returns>The next entry in the enumerator</returns>
        /// <exception cref="InvalidOperationException">If there is no following entry in the enumerator</exception>
        T Peek();

        /// <summary>
        /// Gets the next entry in the enumerator, and caches it so the first following invocation of
        /// <see cref="IEnumerator.MoveNext"/> will simply return the cached value. Does not throw if the enumeration is
        /// finished.
        /// </summary>
        /// <param name="value">
        /// The value to which the next entry in the enumerator will be assigned if the call to <c>TryPeek</c> succeeds
        /// </param>
        /// <returns>
        /// True if the operation succeeded and <paramref name="value"/> is available, false if the enumerator is
        /// finished
        /// </returns>
        bool TryPeek(out T value);
    }

    // https://codereview.stackexchange.com/a/33011
    public class PeekEnumerator<T> : IPeekableEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private T _peek;
        private bool _didPeek;

        public PeekEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        }

        public bool MoveNext()
        {
            if (!_didPeek) return _enumerator.MoveNext();
            
            _didPeek = false;
            return true;

        }

        public void Reset()
        {
            _enumerator.Reset();
            _didPeek = false;
        }

        object IEnumerator.Current => this.Current;
        public T Current => _didPeek ? _peek : _enumerator.Current;
        public void Dispose() => _enumerator.Dispose();

        private void TryFetchPeek()
        {
            if (!_didPeek && (_didPeek = _enumerator.MoveNext()))
            {
                _peek = _enumerator.Current;
            }
        }
        
        public bool TryPeek(out T value)
        {
            TryFetchPeek();
            if (!_didPeek)
            {
                value = default;
                return false;
            }

            value = _peek;
            return true;
        }

        public T Peek()
        {
            return TryPeek(out var value) 
                ? value
                : throw new InvalidOperationException("Enumeration already finished.");
        }
    }
}
