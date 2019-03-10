using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// i can't remember where this is from (it's not in TileCrammer)

namespace TileCrammer
{
    public class DoubleEndedCircularBuffer<T> : IReadOnlyList<T>, ICollection<T>
    {
        public bool IsEmpty => Count == 0;
        public bool IsFull => Count == Capacity;
        
        public int Count { get; private set; }
        public int Capacity { get; }
        public bool IsReadOnly => false;

        private int _head;
        private int _tail;

        private readonly T[] _array;

        public DoubleEndedCircularBuffer(int capacity)
        {
            if (capacity < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    "Must have at least 2 elements to be able to have a separate head and tail");
            }

            Capacity = capacity;
            _array = new T[capacity];
            
            Reset();
        }

        private void Reset()
        {
            _head = _tail = Capacity / 2;
            //GrowForwards();
            GrowBackwards();
        }

        private void GrowForwards()
        {
            _head = (_head + 1) % Capacity;
            if (_head == _tail) ShrinkBackwards();
            Count++;
        }

        private void GrowBackwards()
        {
            _tail = ((_tail - 1) % Capacity + Capacity) % Capacity;
            if (_tail == _head) ShrinkForwards();
            Count++;
        }

        private bool ShrinkForwards()
        {
            var newHead = ((_head - 1) % Capacity + Capacity) % Capacity;
            if (newHead == _tail) return false;
            _head = newHead;
            Count--;
            return true;
        }

        private bool ShrinkBackwards()
        {
            var newTail = _tail = (_tail + 1) % Capacity;
            if (newTail == _head) return false;
            _tail = newTail;
            Count--;
            return true;
        }


        /// <summary>
        /// Inserts the specified element at the front of this buffer.
        /// </summary>
        /// <param name="item"></param>
        public void AddFirst(T item)
        {
            _array[_tail] = item;
            GrowBackwards();
        }
        
        /// <summary>
        /// Inserts the specified element at the end of this buffer.
        /// </summary>
        /// <param name="item"></param>
        public void AddLast(T item)
        {
            _array[_head] = item;
            GrowForwards();
        }

        public T RemoveFirst()
        {
            if (!ShrinkBackwards())
                throw new IndexOutOfRangeException("No items to remove");
            
            var v = _array[_tail];
            _array[_tail] = default;
            return v;
        }

        public T RemoveLast()
        {
            if (!ShrinkForwards())
                throw new IndexOutOfRangeException("No items to remove");

            var v = _array[_head];
            _array[_head] = default;
            return v;
        }

        public bool TryRemoveFirst(out T value)
        {
            if (!ShrinkBackwards())
            {
                value = default;
                return false;
            }

            value = _array[_tail];
            _array[_tail] = default;
            return true;
        }

        public bool TryRemoveLast(out T value)
        {
            if (!ShrinkForwards())
            {
                value = default;
                return false;
            }

            value = _array[_head];
            _array[_head] = default;
            return true;
        }


        public IEnumerator<T> GetEnumerator()
        {
            if (_head > _tail)
            {
                for (var i = _tail; i < _head; i++)
                {
                    yield return _array[i];
                }
            }
            else
            {
                for (var i = _tail; i < _array.Length; i++)
                {
                    yield return _array[i];
                }

                for (var i = 0; i < _head; i++)
                {
                    yield return _array[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item) => AddLast(item);

        public void Clear()
        {
            Array.Clear(_array, 0, Capacity);
            Reset();
        }

        public bool Contains(T item)
        {
            foreach (var e in this)
            {
                if (e == null ? item == null : e.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_head > _tail)
            {
                Array.Copy(_array, _tail, array, arrayIndex, _head - _tail);
            }
            else
            {
                Array.Copy(_array, _tail, array, arrayIndex, _array.Length);
                Array.Copy(_array, 0, array, arrayIndex, _head);
            }
        }

        public bool Remove(T item)
        {
            // FUCK
            throw new InvalidOperationException("FUCK");
        }

        public T this[int index] => _array[(index + _tail) % Capacity];
    }
}
