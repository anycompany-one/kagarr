using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Games;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Games
{
    [V1ApiController]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public ActionResult<List<GameResource>> GetGames()
        {
            return _gameService.GetAllGames()
                .Select(GameResource.FromModel)
                .ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<GameResource> GetGame(int id)
        {
            var game = _gameService.GetGame(id);
            return GameResource.FromModel(game);
        }

        [HttpPost]
        public ActionResult<GameResource> AddGame([FromBody] GameResource resource)
        {
            var game = resource.ToModel();
            var added = _gameService.AddGame(game);
            return Created(new System.Uri($"/api/v1/game/{added.Id}", System.UriKind.Relative), GameResource.FromModel(added));
        }

        [HttpPut("{id:int}")]
        public ActionResult<GameResource> UpdateGame(int id, [FromBody] GameResource resource)
        {
            var game = resource.ToModel();
            game.Id = id;
            var updated = _gameService.UpdateGame(game);
            return GameResource.FromModel(updated);
        }

        [HttpDelete("{id:int}")]
        public ActionResult DeleteGame(int id)
        {
            _gameService.DeleteGame(id);
            return Ok();
        }
    }
}
