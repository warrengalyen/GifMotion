using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GifMotion
{
    public class Helper
    {
        public static readonly string[] ImageFileExtensions =
            {"jpg", "jpeg", "png", "apng", "gif", "bmp", "ico", "tif", "tiff"};

        public static bool IsImage(string filePath)
        {
            return CheckExtension(filePath, ImageFileExtensions);
        }

        public static bool CheckExtension(string filePath, IEnumerable<string> extensions)
        {
            string ext = GetFilenameExtension(filePath);

            return !string.IsNullOrEmpty(ext) &&
                   extensions.Any(x => ext.Equals(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetFilenameExtension(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                int pos = filePath.LastIndexOf('.');

                if (pos >= 0) return filePath.Substring(pos + 1);
            }

            return null;
        }

        public static Image LoadImage(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && IsImage(filePath) && File.Exists(filePath))
                {
                    return Image.FromStream(new MemoryStream(File.ReadAllBytes(filePath)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static void CreateDirectoryFromFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path)) CreateDirectoryFromDirectoryPath(Path.GetDirectoryName(path));
        }

        public static void CreateDirectoryFromDirectoryPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}