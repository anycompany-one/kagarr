using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.History;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.History
{
    [V1ApiController("history")]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public ActionResult<List<HistoryResource>> GetAll([FromQuery] int? gameId)
        {
            var records = gameId.HasValue
                ? _historyService.GetByGameId(gameId.Value)
                : _historyService.GetRecent();

            return records.Select(HistoryResource.FromModel).ToList();
        }
    }
}
