namespace Downla.Models.FileModels
{
    public class ConnectionInfosModel : IComparable<ConnectionInfosModel>
    {
        private Task<HttpResponseMessage>? task;

        public Task<HttpResponseMessage> Task
        {
            get => task ?? throw new ArgumentNullException("Task is null");
            set => task = value;
        }

        public int Index { get; set; }

        public int CompareTo(ConnectionInfosModel? other)
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