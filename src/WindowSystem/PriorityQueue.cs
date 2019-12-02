using System;

namespace NStuff.WindowSystem
{
    internal class PriorityQueue<T> where T : IComparable<T>
    {
        private T[] items = new T[4];

        internal int Count { get; private set; }

        internal T Peek() => items[0];

        internal void Push(T item)
        {
            var items = this.items;
            var count = Count++;
            if (Count == items.Length)
            {
                var t = new T[count * 2];
                Array.Copy(items, t, count);
                this.items = items = t;
            }
            int childIndex = count + 1;
            while (childIndex >= 2)
            {
                var parentIndex = childIndex >> 1;
                var parentItem = items[parentIndex - 1];
                if (item.CompareTo(parentItem) < 0)
                {
                    items[childIndex - 1] = parentItem;
                }
                else
                {
                    break;
                }
                childIndex = parentIndex;
            }
            items[childIndex - 1] = item;
        }

        internal T Pop()
        {
            var items = this.items;
            var result = items[0];
            var count = --Count;
            var lastItem = items[count];
            if (count == 0)
            {
                return result;
            }
            var parentIndex = 0;
            for (;;)
            {
                var childIndex = (parentIndex + 1) << 1;
                if (childIndex > count)
                {
                    break;
                }
                var childItem = items[childIndex - 1];
                if (childIndex < count)
                {
                    var s = items[childIndex];
                    if (s.CompareTo(childItem) < 0)
                    {
                        childIndex++;
                        childItem = s;
                    }
                }
                if (lastItem.CompareTo(childItem) < 0)
                {
                    break;
                }
                items[parentIndex] = childItem;
                parentIndex = childIndex - 1;
            }
            items[parentIndex] = lastItem;
            return result;
        }
    }
}
