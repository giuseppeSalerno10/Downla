namespace Downla.Services.Interfaces
{
    public interface IWritingService
    {
        void WriteBytes(string path, string fileName, long offset, ref byte[] bytes);
        void AppendBytes(string path, string fileName, ref byte[] bytes);
        void Create(string path, string fileName);
        void Delete(string path, string fileName);
        byte[] ReadBytes(string path, string fileName);
        string GeneratePath(string path, string name);
        void Merge(string path, string name);
        void ClearTemp(string path, string name);
    }
}