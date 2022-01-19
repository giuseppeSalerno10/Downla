using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla
{
    public class FilesService
    {
        public static void CreateFile(string path, string fileName) 
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

        public static void AppendBytes(string filePath, byte[] bytes)
        {
            using (var stream = File.Open(filePath, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

    }
}
