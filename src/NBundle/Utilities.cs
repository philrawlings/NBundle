using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NBundle
{
    public static class Utilities
    {
        public static void GetFileList(List<string> fileList, string filePathWithWildcards)
        {
            var directory = Path.GetDirectoryName(filePathWithWildcards);
            var fileName = Path.GetFileName(filePathWithWildcards);

            // Add files to file list
            if (directory.Contains("*"))
                throw new Exception($"Directory wildcards are not currently supported: '{filePathWithWildcards}'");
            if (fileName.Contains("*"))
                fileList.AddRange(Directory.GetFiles(directory, fileName).Select(p => Path.GetFullPath(p)));
            else
                fileList.Add(Path.GetFullPath(Path.Combine(directory, fileName)));
        }

        public static void WriteLine(string message, ConsoleColor color)
        {
            var prevColour = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = prevColour;
        }

        public static void WriteErrorLine(string message, ConsoleColor color)
        {
            var prevColour = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = prevColour;
        }
    }
}
