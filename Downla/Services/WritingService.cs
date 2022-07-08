using Downla.Services.Interfaces;

namespace Downla
{
    public class WritingService : IWritingService
    {
        public void Create(string path, string name)
        {
            Directory.CreateDirectory(path);

            using (var stream = File.Create($"{path}/{name}"))
            {
                stream.Close();
            };
        }
        public void Create(string path, string name, long size)
        {
            Create(path, name);
            while(size > int.MaxValue)
            {
                AppendBytes(path, name, new byte[int.MaxValue]);
                size -= int.MaxValue;
            }
            AppendBytes(path, name, new byte[size]);

        }
        public void Delete(string path, string name)
        {
            File.Delete($"{path}/{name}");
        }
        public void AppendBytes(string path, string name, byte[] bytes)
        {
            using var stream = File.Open($"{path}/{name}", FileMode.Append);
            stream.Write(bytes, 0, bytes.Length);
        }
        public byte[] ReadBytes(string path, string name)
        {
            return File.ReadAllBytes($"{path}/{name}");
        }
        public string GeneratePath(string path, string name)
        {
            return $"{path}/{name}";
        }

        public void WriteBytes(string path, string name, long offset, byte[] bytes)
        {
            using (var stream = new FileStream($"{path}/{name}",FileMode.Open))
            {
                stream.Position = offset;
                stream.Write(bytes,0, bytes.Length);
            }
        }
    }
}