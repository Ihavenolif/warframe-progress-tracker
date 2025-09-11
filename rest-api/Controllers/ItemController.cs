using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("updateDatabase")]
    [SwaggerOperation(Summary = "Triggers an update of the item database", Description = "Sends a request to the data update server to refresh the item database.")]
    [SwaggerResponse(200, "Item database update initiated successfully")]
    [SwaggerResponse(500, "Failed to initiate item database update")]
    [SwaggerResponse(403, "You are not allowed to perform this action")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateItemDatabase()
    {
        var result = await itemService.UpdateItemDatabaseAsync();
        if (result)
        {
            return Ok(new { message = "Item database update initiated successfully." });
        }
        else
        {
            return StatusCode(500, new { message = "Failed to initiate item database update." });
        }
    }
}