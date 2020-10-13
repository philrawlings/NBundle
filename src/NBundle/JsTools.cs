using AutoprefixerHost;
using JavaScriptEngineSwitcher.ChakraCore;
using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NBundle
{
    public static class JsTools
    {
        public static void ProcessJsBundles(JsConfig jsConfig)
        {
            var workingDirectory = Environment.CurrentDirectory;
            foreach (var bundle in jsConfig.Bundles)
            {
                try
                {
                    List<string> filesToAdd = GetInputFiles(bundle);

                    var commentPrefix = string.Empty;
                    var builder = new StringBuilder();
                    if (!bundle.Minify && !string.IsNullOrWhiteSpace(bundle.CommentPrefix))
                    {
                        builder.AppendLine(commentPrefix = "/* " + bundle.CommentPrefix + " */");
                        builder.AppendLine();
                    }

                    foreach (var fileToAdd in filesToAdd)
                    {
                        try
                        {
                            var directory = Path.GetDirectoryName(fileToAdd);
                            var fileName = Path.GetFileName(fileToAdd);
                            Directory.SetCurrentDirectory(directory);

                            var extension = Path.GetExtension(fileName).ToLower();
                            var content = File.ReadAllText(fileName);
                            string compiledContent;
                            switch (extension)
                            {
                                case ".js":
                                    compiledContent = content.Trim();
                                    break;
                                default:
                                    throw new Exception($"File format not supported '{extension}' ('{fileToAdd}')");
                            }

                            if (bundle.Minify)
                                builder.Append(compiledContent);
                            else
                            {
                                if (!string.IsNullOrEmpty(compiledContent))
                                {
                                    if (bundle.AddSourceFilePathComments)
                                        builder.AppendLine($"/* BEGIN {fileName} */");
                                    builder.AppendLine(compiledContent);
                                    if (bundle.AddSourceFilePathComments)
                                        builder.AppendLine($"/* END {fileName} */");
                                    builder.AppendLine();
                                }
                            }
                            Utilities.WriteLine($"[SUCCESS] - Processed file: '{fileToAdd}'.", ConsoleColor.Yellow);
                        }
                        catch (Exception ex)
                        {
                            Utilities.WriteErrorLine($"[FAILURE] - Failed to process file: '{fileToAdd}'.", ConsoleColor.Red);
                            Console.Error.WriteLine(ex.Message);
                        }
                    }

                    Directory.SetCurrentDirectory(workingDirectory);

                    var destFilePath = Path.GetFullPath(bundle.DestinationFilePath);
                    try
                    {
                        if (File.Exists(destFilePath))
                            File.Delete(destFilePath);

                        var destinationFolder = Path.GetDirectoryName(destFilePath);
                        if (!Directory.Exists(destinationFolder))
                            Directory.CreateDirectory(destinationFolder);
                        File.WriteAllText(destFilePath, builder.ToString());
                        Utilities.WriteLine($"[SUCCESS] - Created bundle file: '{destFilePath}'.", ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        Utilities.WriteErrorLine($"[FAILURE] - Failed to create bundle file: '{bundle.DestinationFilePath}' [{destFilePath}].", ConsoleColor.Red);
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                finally
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                    Console.WriteLine();
                }
            }
        }

        private static List<string> GetInputFiles(JsBundle bundle)
        {
            List<string> filesToAdd = new List<string>();
            foreach (var sourceFilePath in bundle.SourceFilePaths)
            {
                Utilities.GetFileList(filesToAdd, sourceFilePath);
            }

            List<string> filesToExclude = new List<string>();
            foreach (var excludesourceFilePath in bundle.ExcludeSourceFilePaths)
            {
                Utilities.GetFileList(filesToExclude, excludesourceFilePath);
            }

            foreach (var excludeFile in filesToExclude)
            {
                var fileToRemove = filesToAdd.Where(f => f.Equals(excludeFile, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (fileToRemove != null)
                {
                    filesToAdd.Remove(fileToRemove);
                }
            }

            return filesToAdd;
        }

        public static List<string> GetDirectoriesToMonitor(JsConfig jsConfig)
        {
            var files = new List<string>();
            foreach (var bundle in jsConfig.Bundles)
                files.AddRange(GetInputFiles(bundle));

            return files.Select(f => Path.GetDirectoryName(f)).Distinct().ToList();
        }
    }
}
