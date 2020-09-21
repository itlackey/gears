using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Gears
{
    public static class ValueExtensions
    {
        public static bool ShouldRunBatch(this IConfigurationSection inputConfigItem)
        {
            return inputConfigItem.GetValue("RunBatch", true);
        }
        public const string FileSuffixDateFormat = "yyyy-MM-dd-HHmm-ffff";

        public static MemoryStream AsUTF8Stream(this string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public static string GenerateFilename(this string reportName, string ext)
        {
            if (!ext.StartsWith("."))
                ext = $".{ext}";

            ext = ext?.ToLower();

            return $"{reportName}-{DateTime.Now.ToString(FileSuffixDateFormat)}{ext}";
        }

        public static string GenerateFilename(this string filenameWithExtension)
        {
            var filename = Path.GetFileNameWithoutExtension(filenameWithExtension);
            var ext = Path.GetExtension(filenameWithExtension);
            return $"{filename}-{DateTime.Now.ToString(FileSuffixDateFormat)}{ext}";
        }
    }

}
