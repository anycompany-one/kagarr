using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.MediaFiles;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.GameFile
{
    [V1ApiController("manualimport")]
    public class ManualImportController : Controller
    {
        private readonly IImportGameFile _importService;

        public ManualImportController(IImportGameFile importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public ActionResult<List<string>> Scan([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is required");
            }

            return _importService.ScanForGameFiles(path);
        }

        [HttpPost]
        public ActionResult<List<ImportResultResource>> Import([FromBody] ManualImportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Path))
            {
                return BadRequest("Path is required");
            }

            List<ImportResult> results;

            if (request.Files != null && request.Files.Count > 0)
            {
                results = request.Files
                    .Select(f => _importService.Import(f, request.GameId))
                    .ToList();
            }
            else
            {
                results = _importService.ImportFolder(request.Path, request.GameId);
            }

            return results.Select(ImportResultResource.FromModel).ToList();
        }
    }

    public class ManualImportRequest
    {
        public string Path { get; set; }
        public int GameId { get; set; }
        public List<string> Files { get; set; }
    }

    public class ImportResultResource
    {
        public bool Success { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public List<string> Errors { get; set; }
        public GameFileResource ImportedFile { get; set; }

        public static ImportResultResource FromModel(ImportResult result)
        {
            return new ImportResultResource
            {
                Success = result.Success,
                SourcePath = result.SourcePath,
                DestinationPath = result.DestinationPath,
                Errors = result.Errors,
                ImportedFile = result.ImportedFile != null
                    ? GameFileResource.FromModel(new Kagarr.Core.Games.GameFile
                    {
                        GameId = result.ImportedFile.GameId,
                        RelativePath = result.ImportedFile.RelativePath,
                        Size = result.ImportedFile.Size,
                        DateAdded = result.ImportedFile.DateAdded,
                        Platform = result.ImportedFile.Platform
                    })
                    : null
            };
        }
    }
}
