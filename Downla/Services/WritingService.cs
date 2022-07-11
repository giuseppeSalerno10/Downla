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
        public void ClearTemp(string path, string name)
        {
            var files = Directory.GetFiles($"{path}/temp");
            foreach (var file in files)
            {
                File.Delete(file);
            }
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

        public void Merge(string path, string name)
        {

        }

        public void WriteBytes(string path, string name, long offset, ref byte[] bytes)
        {
            int fileIndex = (int) ((offset + bytes.Length)/int.MaxValue);
            int localOffset = (int) offset - int.MaxValue * fileIndex;
            if (!Directory.Exists($"{path}/temp"))
            {
                Directory.CreateDirectory($"{path}/temp");
            }
            using (var file = File.Open($"{path}/temp/{name}.temp{fileIndex}", FileMode.OpenOrCreate))
            {
                if(file.Length <  localOffset)
                {
                    file.Position = file.Length;
                    file.Write(new byte[localOffset], 0, localOffset);
                }

                file.Position = localOffset;
                file.Write(bytes, 0, bytes.Length);
            };
        }
    }
}