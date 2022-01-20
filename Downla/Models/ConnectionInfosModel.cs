namespace Downla
{
    internal class ConnectionInfosModel
    {
#pragma warning disable CS8618
        public Task<HttpResponseMessage> Task { get; set; }
#pragma warning restore CS8618

        public int Index { get; set; }
    }
}