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
        public void Delete(string path, string name)
        {
            File.Delete($"{path}/{name}");
        }

        public void AppendBytes(string path, string name, ref byte[] bytes)
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

        public void WriteBytes(string path, string name, long offset, ref byte[] bytes)
        {

            using (var file = File.Open($"{path}/{name}", FileMode.OpenOrCreate))
            {
                if(file.Length < offset)
                {
                    int byteToWrite = (int) (offset - file.Length);
                    file.Position = file.Length;
                    file.Write(new byte[byteToWrite], 0, byteToWrite);
                }

                file.Position = offset;
                file.Write(bytes, 0, bytes.Length);
            }
        }
    }
}