// I dunno I might use it somewhere some day?

    /// <summary>
    /// Defines a one-to-many key/values pair that can be set or retrieved.
    /// </summary>
    /// <typeparam name="TKey">The type of the pair's key.</typeparam>
    /// <typeparam name="TValues">The type of each element in the pair's values.</typeparam>
    public readonly struct OneToManyPair<TKey, TValues> :
        IEquatable<OneToManyPair<TKey, TValues>>,
        IEquatable<KeyValuePair<TKey, IReadOnlyList<TValues>>>,
        IEquatable<KeyValuePair<TKey, IEnumerable<TValues>>>
    {
        /// <summary>
        /// Gets the key in the key/values pair.
        /// </summary>
        public TKey Key { get; }
        
        /// <summary>
        /// Gets the list of values in the key/values pair.
        /// </summary>
        public IReadOnlyList<TValues> Values { get; }

        /// <summary>
        /// Gets the amount of values associated to the key in this key/values pair.
        /// </summary>
        public int ValueCount => Values?.Count ?? 0;

        /// <summary>
        /// Construct an <see cref="OneToManyPair{TKey,TValues}"/> out of a key and an enumerable of values.
        /// </summary>
        /// <param name="key">The key half of the key/value pair.</param>
        /// <param name="values">The values for the key.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> or <paramref name="values"/> is null
        /// </exception>
        public OneToManyPair(TKey key, IEnumerable<TValues> values)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
            Values = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Construct an <see cref="OneToManyPair{TKey,TValues}"/> out of a key and an array of values.
        /// </summary>
        /// <param name="key">The key half of the key/value pair.</param>
        /// <param name="values">The values for the key.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="key"/> or <paramref name="values"/> is null
        /// </exception>
        public OneToManyPair(TKey key, params TValues[] values)
            : this(key, values as IEnumerable<TValues>)
        {
        }
        
        public bool Equals(OneToManyPair<TKey, TValues> other)
            => Key.Equals(other.Key) && Values.SequenceEqual(other.Values);

        public bool Equals(KeyValuePair<TKey, IEnumerable<TValues>> other)
            => Key.Equals(other.Key) && Values.SequenceEqual(other.Value);

        public bool Equals(KeyValuePair<TKey, IReadOnlyList<TValues>> other)
            => Key.Equals(other.Key) && Values.SequenceEqual(other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OneToManyPair<TKey, TValues> other && Equals(other) || 
                   obj is KeyValuePair<TKey, IEnumerable<TValues>> otherKvp && Equals(otherKvp) || 
                   obj is KeyValuePair<TKey, IReadOnlyList<TValues>> otherKvpList && Equals(otherKvpList);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Values != null ? Values.GetHashCode() : 0);
            }
        }

        public override string ToString()
            => $"OneToManyPair<{typeof(TKey)}, {typeof(TValues)}> {{ Key = {Key}; Values = {string.Join(", ", Values)} }}";

        public static bool operator ==(OneToManyPair<TKey, TValues> left, OneToManyPair<TKey, TValues> right)
            => left.Equals(right);
        public static bool operator !=(OneToManyPair<TKey, TValues> left, OneToManyPair<TKey, TValues> right)
            => !left.Equals(right);
        
        public static bool operator ==(KeyValuePair<TKey, IEnumerable<TValues>> left, OneToManyPair<TKey, TValues> right)
            => right.Equals(left);
        public static bool operator !=(KeyValuePair<TKey, IEnumerable<TValues>> left, OneToManyPair<TKey, TValues> right)
            => !right.Equals(left);
        
        public static bool operator ==(OneToManyPair<TKey, TValues> left, KeyValuePair<TKey, IEnumerable<TValues>> right)
            => left.Equals(right);
        public static bool operator !=(OneToManyPair<TKey, TValues> left, KeyValuePair<TKey, IEnumerable<TValues>> right)
            => !left.Equals(right);
        
        public static implicit operator KeyValuePair<TKey, IEnumerable<TValues>>(OneToManyPair<TKey, TValues> from)
            => new KeyValuePair<TKey, IEnumerable<TValues>>(from.Key, from.Values);
        public static implicit operator OneToManyPair<TKey, TValues>(KeyValuePair<TKey, IEnumerable<TValues>> from)
            => new OneToManyPair<TKey, TValues>(from.Key, from.Value);
    }
