using System;
using System.Collections.Generic;
using System.Text;

namespace NBundle
{
    public class JsBundle
    {
        public string DestinationFilePath { get; set; }
        public List<string> SourceFilePaths { get; set; }
        public List<string> ExcludeSourceFilePaths { get; set; }
        public bool Minify { get; set; }
        public string CommentPrefix { get; set; }
        public bool AddSourceFilePathComments { get; set; }
    }
}
