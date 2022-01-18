using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downla.Http
{
    public class MimeMapperService
    {

        private static readonly Dictionary<string, string> _mimeMappingDictionary = new Dictionary<string, string>
        {

            { "x-world/x-3dmf" , ".3dmf" },

            { "text/vnd.abc" , ".abc" },
            { "text/html" , ".acgi" },
            { "text/x-audiosoft-intra" , ".aip" },
            { "text/x-asm" , ".asm" },
            { "text/asp" , ".asp" },
            { "text/x-c" , ".c" },

            { "video/animaflex" , ".afl" },
            { "video/x-ms-asf-plugin" , ".asx" },
            { "video/x-ms-asf" , ".asf" },
            { "video/avi" , ".avi" },
            { "video/avs-video" , ".avs" },

            { "audio/aiff" , ".aiff" },
            { "audio/x-aiff" , ".aiff" },
            { "audio/basic" , ".au" },
            { "audio/x-au" , ".au" },

            { "image/x-jg" , ".art" },
            { "image/bmp" , ".bmp" },
            { "image/x-windows-bmp" , ".bmp" },

            { "application/x-mplayer2" , ".asx" },
            { "application/x-aim" , ".aim" },
            { "application/x-iso9660-image", ".iso" },
            { "application/x-authorware-bin" , ".aab" },
            { "application/x-authorware-map" , ".aam" },
            { "application/x-authorware-seg" , ".aas" },
            { "application/postscript" , ".ai" },
            { "application/x-navi-animation" , ".ani" },
            { "application/x-nokia-9000-communicator-add-on-software" , ".aos" },
            { "application/mime" , ".aps" },
            { "application/arj" , ".arj" },
            { "application/x-bcpio" , ".bcpio" },
            { "application/mac-binary" , ".bin" },
            { "application/book" , ".book" },
            { "application/x-bsh" , ".bsh" },
            { "application/x-bzip" , ".bz" },
            { "application/x-bzip2" , ".bz2" },
            { "application/vnd.ms-pki.seccat" , ".cat" },
            { "application/clariscad" , ".ccad" },
            { "application/x-cocoa" , ".cco" },
            { "application/cdf" , ".cdf" },
            { "application/x-cdf" , ".cdf" },
            { "application/x-x509-ca-cert" , ".cer" },
            { "application/x-chat" , ".chat" },
            { "application/java" , ".class" },
        };

        public static string GetExtension(string mimeType)
        {
            return _mimeMappingDictionary[mimeType] ?? string.Empty;
        }

        public static string GetMimeType(string extension)
        {
            string mimeType = string.Empty;

            foreach (var item in _mimeMappingDictionary)
            {
                if (item.Value.Equals(extension))
                {
                    mimeType = item.Key;
                }
            }

            return mimeType;
        }

    }
}
