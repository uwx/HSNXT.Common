// I dunno I might use it somewhere some day?

    public readonly struct OneToManyPair<TKey, TValues> :
        IEquatable<OneToManyPair<TKey, TValues>>, IEquatable<KeyValuePair<TKey, IEnumerable<TValues>>>
    {
        public TKey Key { get; }
        public IEnumerable<TValues> Values { get; }

        public OneToManyPair(TKey key, IEnumerable<TValues> values)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public OneToManyPair(TKey key, params TValues[] values)
            : this(key, values as IEnumerable<TValues>)
        {
        }
        
        public bool Equals(OneToManyPair<TKey, TValues> other)
            => Key.Equals(other.Key) && Values.SequenceEqual(other.Values);

        public bool Equals(KeyValuePair<TKey, IEnumerable<TValues>> other)
            => Key.Equals(other.Key) && Values.SequenceEqual(other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OneToManyPair<TKey, TValues> other && Equals(other) || 
                   obj is KeyValuePair<TKey, IEnumerable<TValues>> otherKvp && Equals(otherKvp);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Key.GetHashCode() * 397) ^ Values.GetHashCode();
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
