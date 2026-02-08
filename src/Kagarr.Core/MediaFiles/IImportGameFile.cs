using System.Collections.Generic;

namespace Kagarr.Core.MediaFiles
{
    public interface IImportGameFile
    {
        ImportResult Import(string sourcePath, int gameId);
        List<ImportResult> ImportFolder(string folderPath, int gameId);
        List<string> ScanForGameFiles(string path);
    }
}
