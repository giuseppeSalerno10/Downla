namespace Downla.Services.Interfaces
{
    public interface IWritingService
    {
        void WriteBytes(string path, string fileName, long offset, byte[] bytes);
        void AppendBytes(string path, string fileName, byte[] bytes);
        void Create(string path, string fileName);
        void Create(string path, string fileName, long size);
        void Delete(string path, string fileName);
        byte[] ReadBytes(string path, string fileName);
        string GeneratePath(string path, string name);
    }
}