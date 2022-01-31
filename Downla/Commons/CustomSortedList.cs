using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla
{
    public class CustomSortedList<T> :IDisposable where T : IComparable<T>
    {

        public int Count { get => internalList.Count; }

        private List<T> internalList = new List<T>();

        public CustomSortedList() : base() { }

        public void Insert(T item)
        {
            int index;

            if (internalList.Count == 0 || item.CompareTo(internalList[0]) < 0)
            {
                index = 0;
            }
            else if (item.CompareTo(internalList[^1]) == 1)
            {
                index = internalList.Count;
            }
            else
            {
                index = ALittleBitDifferentBinarySearch(item);
            }

            internalList.Insert(index, item);
            
        }

        public void Add(T item)
        {

            internalList.Add(item);

        }

        public void Remove(T item)
        {
            var index = ALittleBitDifferentBinarySearch(item);

            internalList.RemoveAt(index);
        }

        public T[] ToArray()
        {
            return internalList.ToArray();
        }

        public void Dispose()
        {
            internalList.Clear();
        }


        private int ALittleBitDifferentBinarySearch(T item)
        {
            int upperIndex = internalList.Count - 1;
            int lowerIndex = 0;

            while (lowerIndex != upperIndex)
            {
                int middleIndex = (upperIndex + lowerIndex) / 2;
                if(item.CompareTo(internalList[middleIndex]) == 1)
                {
                    lowerIndex = middleIndex + 1;
                }
                else
                {
                    upperIndex = middleIndex;
                }
            }

            return lowerIndex;
        }


    }
}
