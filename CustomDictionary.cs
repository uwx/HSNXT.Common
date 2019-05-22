using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

// Extracted from a version of HSNXT.DSharpPlus (private branch) at May 22nd, 2019.
// This class was heavily refactored but was eventually made unnecessary, so here is its code.
// This dictionary uses much of the code from MapReduce.NET, under an unknown open-source license.
// Keep the licensing issues in mind if you intend on consuming this code. The code was originally found at:
// http://blog.teamleadnet.com/2012/07/ultra-fast-hashtable-dictionary-with.html

namespace HSNXT.DSharpPlus.CNextNotifier
{
    internal static class CustomDictionary
    {
        internal static readonly uint[] PrimeSizes =
        {
            89, 179, 359, 719, 1439, 2879, 5779, 11579, 23159, 46327,
            92657, 185323, 370661, 741337, 1482707, 2965421, 5930887, 11861791,
            23723599, 47447201, 94894427, 189788857, 379577741, 759155483
        };
    }
    
    internal class CustomDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private struct DictionaryEntry
        {
            public int Next;
            public readonly TKey Key;
            public TValue Value;
            public readonly uint HashCode;

            public DictionaryEntry(int next, TKey key, TValue value, uint hashCode)
            {
                Key = key;
                Next = next;
                Value = value;
                HashCode = hashCode;
            }
        }

        public readonly struct EntryReference
        {
            internal readonly int Pos;

            internal EntryReference(int pos) => Pos = pos;
        }

        private int[] _hashes;
        private DictionaryEntry[] _entries;

        private const int InitialSize = 89;
        //private const float Loadfactor = 1f;

        //int maxitems = (int)( initialsize * loadfactor );

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public TValue this[[NotNull] TKey key]
        {
            get => Get(key);
            set => Add(key, value, true);
        }

        public TValue this[EntryReference reference]
        {
            get => _entries[reference.Pos].Value;
            set => _entries[reference.Pos].Value = value;
        }

        public CustomDictionary(int initialSize = InitialSize)
        {
            Initialize(initialSize);
        }

        public EntryReference InitOrGet([NotNull] TKey key, TValue initialValue = default)
            => Add(key, initialValue, false);

        public EntryReference Add([NotNull] TKey key, TValue value, bool overwrite)
        {
            if (Count >= _entries.Length) Resize();

            var hash = (uint) key.GetHashCode();
            var hashPos = hash % (uint) _hashes.Length;
            var entryLocation = _hashes[hashPos];
            var storePos = Count;

            if (entryLocation != -1) // already there
            {
                var currEntryPos = entryLocation;

                do
                {
                    var entry = _entries[currEntryPos];

                    // same key is in the dictionary
                    if (key.Equals(entry.Key))
                    {
                        if (!overwrite)
                            return new EntryReference(currEntryPos);

                        storePos = currEntryPos;
                        break; // do not increment nextfree - overwriting the value
                    }

                    currEntryPos = entry.Next;
                } while (currEntryPos > -1);

                Count++;
            }
            else // new value
            {
                //hashcount++;
                Count++;
            }

            _hashes[hashPos] = storePos;

            _entries[storePos] = new DictionaryEntry(entryLocation, key, value, hash);
            
            return new EntryReference(storePos);
        }

        private void Resize()
        {
            var newsize = FindNewSize();
            var newhashes = new int[newsize];
            var newentries = new DictionaryEntry[newsize];

            Array.Copy(_entries, newentries, Count);

            for (var i = 0; i < newsize; i++)
            {
                newhashes[i] = -1;
            }

            for (var i = 0; i < Count; i++)
            {
                var pos = newentries[i].HashCode % newsize;
                var prevpos = newhashes[pos];
                newhashes[pos] = i;

                if (prevpos != -1)
                    newentries[i].Next = prevpos;
            }

            _hashes = newhashes;
            _entries = newentries;

            //maxitems = (int) (newsize * loadfactor );
        }

        private uint FindNewSize()
        {
            var roughsize = (uint) _hashes.Length * 2 + 1;

            foreach (var t in CustomDictionary.PrimeSizes)
            {
                if (t >= roughsize)
                    return t;
            }

            throw new ArgumentOutOfRangeException(nameof(CustomDictionary.PrimeSizes), "Too large array");
        }

        public TValue Get([NotNull] TKey key)
        {
            var pos = GetPosition(key);

            if (pos == -1)
                throw new ArgumentException("Key does not exist", nameof(key));

            return _entries[pos].Value;
        }

        public int GetPosition([NotNull] TKey key)
        {
            var hash = (uint) key.GetHashCode();

            var pos = hash % (uint) _hashes.Length;

            var entryLocation = _hashes[pos];

            if (entryLocation == -1)
                return -1;

            var nextpos = entryLocation;

            do
            {
                var entry = _entries[nextpos];

                if (key.Equals(entry.Key))
                    return nextpos;

                nextpos = entry.Next;
            } while (nextpos != -1);

            return -1;
        }

        public bool ContainsKey([NotNull] TKey key)
        {
            return GetPosition(key) != -1;
        }

        public bool TryGetValue([NotNull] TKey key, out TValue value)
        {
            var pos = GetPosition(key);

            if (pos == -1)
            {
                value = default;
                return false;
            }

            value = _entries[pos].Value;

            return true;
        }

        public void Clear() => Initialize();

        private void Initialize(int size = InitialSize)
        {
            _hashes = new int[size];
            _entries = new DictionaryEntry[size];
            Count = 0;

            for (var i = 0; i < _entries.Length; i++)
            {
                _hashes[i] = -1;
            }
        }

        public bool Contains(TKey key, TValue value)
        {
            if (key == null)
                return false;

            return TryGetValue(key, out var foundValue) && Equals(value, foundValue);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(_entries[i].Key, _entries[i].Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
