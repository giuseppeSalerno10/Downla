namespace Downla
{
    public class ConnectionInfosModel : IComparable<ConnectionInfosModel>
    {
        private Task<HttpResponseMessage>? task;

        public Task<HttpResponseMessage> Task 
        { 
            get => task ?? throw new ArgumentNullException("Task is null"); 
            set => task = value; }

        public int Index { get; set; }

        public int CompareTo(ConnectionInfosModel? other)
        {
            return Index > other?.Index ? 1 : 0;
        }
    }
}