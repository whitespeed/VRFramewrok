using System;
using System.Collections.Generic;

namespace Framework
{
    public class PriorityQueue<TKey,TValue>
    {
        public delegate bool Comparable(TKey left, TKey right);
        protected List<TKey> priority = new List<TKey>();
        protected List<TValue> data = new List<TValue>();
        protected Comparable compare;
        public PriorityQueue(Comparable comparable)
        {
            compare = comparable;
        } 

        public TValue Dequeue()
        {
            if (Count <= 0)
                throw new ArgumentOutOfRangeException();
            var min = data[0];
            Swap(0, Count - 1);
            priority.RemoveAt(Count - 1);
            data.RemoveAt(Count - 1);
            if (Count > 0)
                DownHeap(0);
            return min;
        }

        public TValue Peek()
        {
            if (Count <= 0)
                throw new ArgumentOutOfRangeException();
            return data[0];
        }

        public void Enqueue(TKey p,TValue d)
        {
            var index = Count;
            data.Add(d);
            priority.Add(p);
            UpHeap(index);
        }

        public int Count
        {
            get { return data.Count; }
        }

        public void Clear()
        {
            priority.Clear();
            data.Clear();
        }

        protected void UpHeap(int start)
        {
            var i = start;
            var j = Parent(i);
            var tmp = data[i];
            while (j >= 0)
            {
                if (Compare(j, i))
                    break;
                Swap(i, j);
                i = j;
                j = Parent(i);
            }
            data[i] = tmp;
        }

        protected int Parent(int i)
        {
            if (i - 1 < 0) return -1;
            return (i - 1)/2;
        }

        protected void DownHeap(int start)
        {
            var i = start;
            var tmp = data[i];
            var j = i*2 + 1;
            while (j < Count)
            {
                if (j + 1 < Count && Compare(j + 1, j))
                    j++;
                if (Compare(i, j))
                    break;
                Swap(i, j);
                i = j;
                j = 2*i + 1;
            }
            data[i] = tmp;
        }

        protected bool Compare(int indexL, int indexR)
        {
            return compare(priority[indexL] ,priority[indexR]);
        }

        protected void Swap(int indexL, int indexR)
        {
            var priTmp = priority[indexL];
            priority[indexL] = priority[indexR];
            priority[indexR] = priTmp;

            var tmp = data[indexL];
            data[indexL] = data[indexR];
            data[indexR] = tmp;
        }
    }
}
