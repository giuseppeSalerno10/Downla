namespace Downla.Services.Interfaces
{
    public interface IWritingService
    {
        void AppendBytes(string path, string fileName, byte[] bytes);
        void Create(string path, string fileName);
        void Delete(string path, string fileName);
        byte[] ReadBytes(string path, string fileName);
        string GeneratePath(string path, string name);
    }
}