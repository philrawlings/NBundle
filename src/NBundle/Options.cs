using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBundle
{
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "JSON configuration filename", Default = "nbundle.json")]
        public string ConfigFilename { get; set; }

        [Option('w', "watch", Required = false, HelpText = "Run in watch mode")]
        public bool Watch { get; set; }
    }
}
