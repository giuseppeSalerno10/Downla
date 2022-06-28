namespace Downla.Services.Interfaces
{
    public interface IWritingService
    {
        public string WritePath { get; set; }

        void AppendBytes(string fileName, byte[] bytes);
        void Create(string fileName);
        void Delete(string fileName);
        byte[] ReadBytes(string fileName);
        string GeneratePath(string name);
    }
}