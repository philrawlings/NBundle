using System;
using System.Collections.Generic;
using System.Text;

namespace NBundle
{
    public class CssBundle
    {
        public string DestinationFilePath { get; set; }
        public List<string> SourceFilePaths { get; set; } = new List<string>();
        public List<string> ExcludeSourceFilePaths { get; set; } = new List<string>();
        public bool Minify { get; set; }
        public string CommentPrefix { get; set; }
        public bool AddSourceFilePathComments { get; set; }
    }
}
