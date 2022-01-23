using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla
{
    public class CustomSortedList<T> where T : IComparable<T>
    {
        private List<T> internalList = new List<T>();

        public CustomSortedList() : base() { }

        public void Insert(T item)
        {
            var index = SearchIndex(item);

            internalList.Insert(index, item);
            
        }
        public void Clear()
        {
            internalList.Clear();
        }

        public void Remove(T item)
        {
            var index = SearchIndex(item);

            internalList.RemoveAt(index);
        }

        public T[] ToArray()
        {
            return internalList.ToArray();
        }

        private int SearchIndex(T item)
        {
            //Temporary Solution, O(n), it needs to be O(logn)

            var index = 0;
            for (var i = 0; i < internalList.Count; i++)
            {
                if(item.CompareTo(internalList[i]) > 0)
                {
                    index = i + 1;
                }
            }
            return index;
        }



    }
}
