using System.Collections.Generic;
using Kagarr.Core.Games;

namespace Kagarr.Core.MediaFiles
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public Game Game { get; set; }
        public GameFile ImportedFile { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
