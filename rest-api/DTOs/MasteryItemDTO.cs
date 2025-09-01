using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace rest_api.DTO;

public class MasteryItemDTO
{
    public string? username { get; set; }
    public int? xpGained { get; set; }
    public int? xpRequired { get; set; }


    public string? itemName { get; set; }
    public string? itemType { get; set; }
    public string? itemClass { get; set; }
    public string? uniqueName { get; set; }

}

public class MasteryItemWithComponentsDTO : MasteryItemDTO
{
    public MasteryItemWithComponentsDTO(MasteryItemDTO masteryItemDTO)
    {
        this.username = masteryItemDTO.username;
        this.xpGained = masteryItemDTO.xpGained;
        this.xpRequired = masteryItemDTO.xpRequired;
        this.itemName = masteryItemDTO.itemName;
        this.itemType = masteryItemDTO.itemType;
        this.itemClass = masteryItemDTO.itemClass;
        this.uniqueName = masteryItemDTO.uniqueName;
    }

    public List<ComponentItemDTO>? components { get; set; }
}

public class ComponentItemDTO
{
    public string? name { get; set; }
    public string? uniqueName { get; set; }
    public int? countOwned { get; set; }
    public int? countRequired { get; set; }
    public bool? isCraftable { get; set; }
    public bool? recipeOwned { get; set; }
}

public class UnownedItemDTO
{
    public string? unique_name { get; set; }
    public string? components_json { get; set; }

    [NotMapped]
    public List<ComponentItemDTO>? components =>
        string.IsNullOrWhiteSpace(components_json)
            ? new List<ComponentItemDTO>()
            : JsonSerializer.Deserialize<List<ComponentItemDTO>>(components_json);
}

