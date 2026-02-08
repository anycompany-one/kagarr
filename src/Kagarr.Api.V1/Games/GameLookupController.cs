using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.MediaCovers;
using Kagarr.Core.MetadataSource;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Games
{
    [V1ApiController("game/lookup")]
    public class GameLookupController : Controller
    {
        private readonly ISearchForNewGame _searchProxy;
        private readonly IMapCoversToLocal _coverMapper;

        public GameLookupController(ISearchForNewGame searchProxy, IMapCoversToLocal coverMapper)
        {
            _searchProxy = searchProxy;
            _coverMapper = coverMapper;
        }

        [HttpGet]
        public ActionResult<List<GameResource>> Search([FromQuery] string term)
        {
            var igdbResults = _searchProxy.SearchForNewGame(term);
            return MapToResource(igdbResults).ToList();
        }

        [HttpGet("{igdbId:int}")]
        public ActionResult<GameResource> GetByIgdbId(int igdbId)
        {
            var game = _searchProxy.GetGameInfo(igdbId);

            if (game == null)
            {
                return NotFound();
            }

            var resource = GameResource.FromModel(game);
            _coverMapper.ConvertToLocalUrls(resource.Id, game.Images);

            return resource;
        }

        private IEnumerable<GameResource> MapToResource(IEnumerable<Kagarr.Core.Games.Game> games)
        {
            foreach (var game in games)
            {
                var resource = GameResource.FromModel(game);

                // For lookup results, the game is not in the library yet (Id=0),
                // so ConvertToLocalUrls will keep the remote URLs as-is
                _coverMapper.ConvertToLocalUrls(resource.Id, game.Images);

                yield return resource;
            }
        }
    }
}
