﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Files
{
    public class FilesService
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

        public async Task AppendBytesAsync(string filePath, byte[] bytes)
        {
            using (var stream = File.Open(filePath, FileMode.Append))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

    }
}
