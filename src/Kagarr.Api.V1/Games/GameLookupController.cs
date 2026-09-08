using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<ActionResult<List<GameResource>>> Search([FromQuery] string term)
        {
            var igdbResults = await _searchProxy.SearchForNewGameAsync(term);

            var resources = new List<GameResource>();
            foreach (var game in igdbResults)
            {
                var resource = GameResource.FromModel(game);

                // For lookup results, the game is not in the library yet (Id=0),
                // so ConvertToLocalUrlsAsync will keep the remote URLs as-is
                await _coverMapper.ConvertToLocalUrlsAsync(resource.Id, game.Images);

                resources.Add(resource);
            }

            return resources;
        }

        [HttpGet("{igdbId:int}")]
        public async Task<ActionResult<GameResource>> GetByIgdbId(int igdbId)
        {
            var game = await _searchProxy.GetGameInfoAsync(igdbId);

            if (game == null)
            {
                return NotFound();
            }

            var resource = GameResource.FromModel(game);
            await _coverMapper.ConvertToLocalUrlsAsync(resource.Id, game.Images);

            return resource;
        }
    }
}
