using Downla.Services.Interfaces;

namespace Downla
{
    public class WritingService : IWritingService
    {
        public string WritePath { get; set; } = null!;

        public void Create(string name)
        {

            Directory.CreateDirectory(WritePath);

            using (var stream = File.Create($"{WritePath}/{name}"))
            {
                stream.Close();
            };
        }
        public void Delete(string name)
        {
            File.Delete($"{WritePath}/{name}");
        }
        public void AppendBytes(string name, byte[] bytes)
        {
            using var stream = File.Open($"{WritePath}/{name}", FileMode.Append);
            stream.Write(bytes, 0, bytes.Length);
        }
        public byte[] ReadBytes(string name)
        {
            return File.ReadAllBytes($"{WritePath}/{name}");
        }
        public string GeneratePath(string name)
        {
            return $"{WritePath}/{name}";
        }
    }
}