using Microsoft.AspNetCore.Mvc;
using rest_api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace rest_api.Controllers;

[ApiController]
[Route("api/items")]
public class ItemController : ControllerBase
{
    private readonly IItemService itemService;
    private readonly IWarframePublicExportService warframePublicExportService;

    public ItemController(IItemService itemService, IWarframePublicExportService warframePublicExportService)
    {
        this.itemService = itemService;
        this.warframePublicExportService = warframePublicExportService;
    }

    [HttpGet("index")]
    [SwaggerOperation(Summary = "Gets Warframe Public Export index", Description = "Fetches and returns the Warframe Public Export index.")]
    [SwaggerResponse(200, "Index retrieved successfully", typeof(Dictionary<string, string>))]
    public ActionResult<Dictionary<string, string>> Index()
    {
        var index = warframePublicExportService.GetIndex().Result;
        return Ok(index);
    }
}