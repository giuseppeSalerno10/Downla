namespace Downla.Models.FileModels
{
    public class ConnectionInfosModel<TModel> : IComparable<ConnectionInfosModel<TModel>>
    {
        public Task<TModel> Task { get; set; } = null!;

        public int Index { get; set; }

        public int CompareTo(ConnectionInfosModel<TModel>? other)
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