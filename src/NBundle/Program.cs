using AutoprefixerHost;
using CommandLine;
using JavaScriptEngineSwitcher.ChakraCore;
using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace NBundle
{
    class Program
    {
        static volatile bool changed = false;

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed<Options>(opts =>
            {
                var configJson = File.ReadAllText(opts.ConfigFilename);
                var config = JsonSerializer.Deserialize<Config>(configJson, options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                CssTools.ProcessCssBundles(config.Css);
                JsTools.ProcessJsBundles(config.Js);

                if (opts.Watch)
                {
                    var cts = new CancellationTokenSource();

                    Console.CancelKeyPress += (sender, eventArgs) => {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    var watchers = new List<FileSystemWatcher>();

                    try
                    {
                        var directoriesToMonitor = new List<string>();
                        directoriesToMonitor.AddRange(CssTools.GetDirectoriesToMonitor(config.Css));
                        directoriesToMonitor.AddRange(JsTools.GetDirectoriesToMonitor(config.Js));

                        foreach (var dir in directoriesToMonitor)
                        {
                            var watcher = new FileSystemWatcher();
                            watcher.Path = dir;
                            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                            watcher.Filter = "*.*";
                            watcher.Changed += (sender, e) =>
                            {
                                changed = true;
                            };
                            watcher.EnableRaisingEvents = true;
                            Utilities.WriteLine($"[STARTING] - Directory watcher '{dir}'.", ConsoleColor.Gray);
                        }

                        Console.WriteLine("Directory watchers running. Press CTRL+C to exit.");

                        while(!cts.IsCancellationRequested)
                        {
                            cts.Token.WaitHandle.WaitOne(200); // Check every 500 ms for updates
                            if (changed)
                            {
                                Thread.Sleep(300); // Wait another 300ms for further updates
                                changed = false; // Set flag to false before processing starts in case another modification is made during processing.

                                // Crude implementation at present - any changes to monitored directories forces regeneration of css/js
                                CssTools.ProcessCssBundles(config.Css);
                                JsTools.ProcessJsBundles(config.Js);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.WriteErrorLine($"[CRITICAL] - Directory watchers failed to start.", ConsoleColor.Red);
                        Console.Error.Write(ex.Message);
                    }
                    finally
                    {
                        foreach (var watcher in watchers)
                        {
                            watcher?.Dispose();
                        }
                    }

                    // Wait for exit
                    cts.Token.WaitHandle.WaitOne();
                }

            });
        }
    }
}
