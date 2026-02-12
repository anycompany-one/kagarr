using System.Collections.Generic;

namespace Kagarr.Core.MediaFiles
{
    public interface IImportGameFile
    {
        ImportResult Import(string sourcePath, int gameId, TransferMode transferMode = TransferMode.Move);
        List<ImportResult> ImportFolder(string folderPath, int gameId, TransferMode transferMode = TransferMode.Move);
        List<string> ScanForGameFiles(string path);
    }
}
