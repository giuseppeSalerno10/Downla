namespace Downla.Services.Interfaces
{
    public interface IFilesService
    {
        void AppendBytes(string filePath, byte[] bytes);
        void CreateFile(string path, string fileName);
        void DeleteFile(string path, string fileName);
    }
}