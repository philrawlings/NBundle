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
    public static class CssTools
    {
        public static void ProcessCssBundles(CssConfig cssConfig)
        {
            var workingDirectory = Environment.CurrentDirectory;
            foreach (var bundle in cssConfig.Bundles)
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
                                case ".scss":
                                    var options = new CompilationOptions
                                    {
                                        IndentType = IndentType.Space,
                                        IndentWidth = 2,
                                        LineFeedType = LineFeedType.Lf,
                                        OutputStyle = OutputStyle.Nested,
                                        Precision = 5,
                                        SourceMap = true,
                                        InlineSourceMap = false
                                    };
                                    if (bundle.Minify)
                                        options.OutputStyle = OutputStyle.Compressed;

                                    var compiledResult = SassCompiler.Compile(content, options: options);
                                    compiledContent = compiledResult.CompiledContent;
                                    break;
                                case ".css":
                                    compiledContent = content;
                                    break;
                                default:
                                    throw new Exception($"File format not supported '{extension}' ('{fileToAdd}')");
                            }

                            var autoPrefixer = new Autoprefixer(new ChakraCoreJsEngineFactory(), new ProcessingOptions
                            {
                            });
                            var processedResult = autoPrefixer.Process(compiledContent);
                            var processedContent = processedResult.ProcessedContent.Trim();
                            if (bundle.Minify)
                                builder.Append(processedContent);
                            else
                            {
                                if (!string.IsNullOrEmpty(processedContent))
                                {
                                    if (bundle.AddSourceFilePathComments)
                                        builder.AppendLine($"/* BEGIN {fileName} */");
                                    builder.AppendLine(processedContent);
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

        private static List<string> GetInputFiles(CssBundle bundle)
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

        public static List<string> GetDirectoriesToMonitor(CssConfig cssConfig)
        {
            var files = new List<string>();
            foreach (var bundle in cssConfig.Bundles)
                files.AddRange(GetInputFiles(bundle));

            return files.Select(f => Path.GetDirectoryName(f)).Distinct().ToList();
        }
    }
}
