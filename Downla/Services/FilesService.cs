namespace Downla
{
    public class FilesService : IFilesService
    {
        public void CreateFile(string path, string fileName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var stream = File.Create($"{path}/{fileName}"))
            {
                stream.Close();
            };
        }
        public void DeleteFile(string path, string fileName)
        {
            File.Delete($"{path}/{fileName}");
        }
        public void AppendBytes(string filePath, byte[] bytes)
        {
            using var stream = File.Open(filePath, FileMode.Append);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}