using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

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

public class MasteryItemNewDTO
{
    public string? itemName { get; set; }
    public string? itemType { get; set; }
    public string? itemClass { get; set; }
    public string? uniqueName { get; set; }
    public string? recipeName { get; set; }
    public string? recipeUniqueName { get; set; }
    public int? xpRequired { get; set; }

    [JsonExtensionData]
    [NotMapped]
    public Dictionary<string, PlayerMasteryItemDTO> players { get; set; } = new Dictionary<string, PlayerMasteryItemDTO>();
    [NotMapped]
    public List<string>? playerNames => players?.Keys.OrderBy(n => n).ToList();
}

public class PlayerMasteryItemDTO
{
    public int? xpGained { get; set; }
    public bool? blueprintOwned { get; set; }
    [JsonIgnore]
    public string? components_json { get; set; }

    [NotMapped]
    public List<ComponentItemDTO>? components =>
        string.IsNullOrWhiteSpace(components_json)
            ? new List<ComponentItemDTO>()
            : JsonSerializer.Deserialize<List<ComponentItemDTO>>(components_json);
}