using System.Runtime.InteropServices;

namespace Downla.Models.FileModels
{
    public class IndexedItem<TModel> : IndexedItem
    {
        private TModel data = default!;
        public TModel Data { get => data; set => data = value; }
    }
    public class IndexedItem : IComparable<IndexedItem>
    {
        public int Index { get; set; }

        public int CompareTo(IndexedItem? other)
        {
            int comparisonValue;

            if (Index > other?.Index)
            {
                comparisonValue = 1;
            }
            else if (Index == other?.Index)
            {
                comparisonValue = 0;
            }
            else
            {
                comparisonValue = -1;
            }

            return comparisonValue;
        }
    }
}