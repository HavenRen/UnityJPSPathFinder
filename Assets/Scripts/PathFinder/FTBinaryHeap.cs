using System;

namespace FT
{
    /// <summary>
    /// 最小二叉堆
    /// </summary>
    public class FTBinaryHeap<T> where T : IComparable<T>
    {
        T[] items;
        public int Count { get; private set; }

        public FTBinaryHeap() : this(50) { }

        public FTBinaryHeap(int size)
        {
            if (size < 0)
            {
                throw new IndexOutOfRangeException();
            }
            items = new T[size];
        }

        public void Enqueue(T value)
        {
            if (Count == items.Length)
            {
                Resize(Count * 2);
            }
            items[Count] = value;
            ++Count;
            UpAdjust(Count - 1);
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException();
            }
            var result = items[0];
            --Count;
            if (Count > 0)
            {
                items[0] = items[Count];
                items[Count] = default;
                DownAdjust();
            }
            return result;
        }

        public T RemoveAtEnd()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException();
            }
            var result = items[Count - 1];
            items[Count - 1] = default;
            --Count;
            return result;
        }

        void Resize(int newSize)
        {
            var temp = new T[newSize];
            Array.Copy(items, 0, temp, 0, Count);
            items = temp;
        }

        /// <summary>
        /// 下沉调整 用于删除堆顶
        /// </summary>
        void DownAdjust()
        {
            var parent = 0;
            var left = (parent * 2) + 1;  // left index
            while (left < Count)
            {
                var right = left + 1;  // right index (parent + 1) * 2
                var t1 = items[right];
                var t2 = items[left];
                var min = (right < Count && t1.CompareTo(t2) < 0) ? right : left;
                t1 = items[min];
                t2 = items[parent];
                if (t1.CompareTo(t2) < 0)
                {
                    items[parent] = t1;
                    items[min] = t2;
                    parent = min;
                    left = (parent * 2) + 1;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 上浮调整 用于插入
        /// </summary>
        void UpAdjust(int index)
        {
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                var t1 = items[index];
                var t2 = items[parent];
                if (t1.CompareTo(t2) < 0)
                {
                    items[index] = t2;
                    items[parent] = t1;
                }
                else
                {
                    break;
                }
                index = parent;
            }
        }

        /// <summary>
        /// 如果t的value变小了 尝试上浮调整
        /// </summary>
        public void TryUpAdjust(T t)
        {
            int index = GetIndex(t);
            UpAdjust(index);
        }

        int GetIndex(T value)
        {
            var count = items.Length;
            for (int i = 0; i < count; i++)
            {
                if (ReferenceEquals(items[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}