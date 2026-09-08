using Kagarr.Core.MediaCovers;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.MediaCovers
{
    [V1ApiController("mediacover")]
    public class MediaCoverController : Controller
    {
        private readonly IMapCoversToLocal _coverMapper;

        public MediaCoverController(IMapCoversToLocal coverMapper)
        {
            _coverMapper = coverMapper;
        }

        [HttpGet("{gameId:int}/{fileName}")]
        public ActionResult GetMediaCover(int gameId, string fileName)
        {
            // GetStoredCoverPath rejects traversal attempts (separators, '..', rooted
            // paths) and returns null when the file does not exist.
            var path = _coverMapper.GetStoredCoverPath(gameId, fileName);

            if (path == null)
            {
                return NotFound();
            }

            return PhysicalFile(path, GetContentType(fileName));
        }

        private static string GetContentType(string fileName)
        {
            var extension = global::System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
