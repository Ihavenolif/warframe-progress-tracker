using System.Text.Json.Nodes;
using rest_api.Models;
using SharpCompress.Compressors.LZMA;

namespace rest_api.Services;

public interface IWarframePublicExportService
{
    public Task<Dictionary<string, string>> GetIndex();
    public Task<List<Item>> GetWarframes(Dictionary<string, string> index);
}

public class WarframePublicExportService : IWarframePublicExportService
{
    private static Dictionary<string, string> ItemClasses = new Dictionary<string, string> {
        {"LongGuns", "Primary"},
        {"Pistols", "Secondary"},
        {"Melee", "Melee"},
        {"Suits", "Warframe"},
        {"Kdrive", "K-Drive"},
        {"MechSuits", "Necramech"},
        {"SpaceSuits", "Archwing"},
        {"Amp", "Amp"},
        {"Kitgun", "Kitgun"},
        {"OperatorAmps", "Amp"},
        {"Zaw", "Zaw"},
        {"SpaceMelee", "Archmelee"},
        {"Hound", "Hound"},
        {"KubrowPets", "Pet"},
        {"Moa", "Moa"},
        {"Sentinels", "Sentinel"},
        {"SentinelWeapons", "Sentinel Weapon"},
        {"SpaceGuns", "Archgun"}
    };

    private static Dictionary<string, string> ItemsToReplace = new Dictionary<string, string>
    {
        {"/Lotus/Types/Recipes/Weapons/GrineerCombatKnifeSortieBlueprint","/Lotus/Types/Recipes/Weapons/GrineerCombatKnifeBlueprint"}, // Sheev
        {"/Lotus/Types/Recipes/Weapons/DetronBlueprint", "/Lotus/Types/Recipes/Weapons/CorpusHandcannonBlueprint"} // Detron
    };

    private static List<string> ItemsToIgnore = new List<string>
    {
        "/Lotus/Types/Recipes/Weapons/LowKatanaBlueprint",
        "/Lotus/Types/Recipes/Weapons/GrineerHandcannonBlueprint",
        "/Lotus/Types/Recipes/Weapons/CorpusHandcannonBlueprint",
        "/Lotus/Types/Recipes/Weapons/GrineerCombatKnifeBlueprint"
    };

    static async Task<string> GetRawIndex()
    {
        string url = "https://origin.warframe.com/PublicExport/index_en.txt.lzma";
        using HttpClient client = new HttpClient();
        byte[] compressedData = await client.GetByteArrayAsync(url);

        using MemoryStream input = new MemoryStream(compressedData);
        using MemoryStream output = new MemoryStream();

        // Read the first 5 bytes: properties
        byte[] properties = new byte[5];
        input.Read(properties, 0, 5);

        // Read the next 8 bytes: uncompressed size (little endian)
        byte[] sizeBytes = new byte[8];
        input.Read(sizeBytes, 0, 8);
        long outSize = BitConverter.ToInt64(sizeBytes, 0);

        // Decode
        Decoder decoder = new Decoder();
        decoder.SetDecoderProperties(properties);
        decoder.Code(input, output, input.Length - input.Position, outSize, null);

        return System.Text.Encoding.UTF8.GetString(output.ToArray());
    }

    public async Task<Dictionary<string, string>> GetIndex()
    {
        var indexRaw = await GetRawIndex();
        var ret = new Dictionary<string, string>();

        var lines = indexRaw.Split("\r\n");
        foreach (var line in lines)
        {
            if (line.Contains("Manifest"))
            {
                var innerKey = line.Split('.')[0].Substring(6); // Remove "Export" prefix
                var innerValue = line;
                ret[innerKey] = innerValue;
                continue;
            }

            var key = line.Split('_')[0].Substring(6);
            var value = line;
            ret[key] = value;
        }

        return ret;
        throw new NotImplementedException();
    }

    public async Task<List<Item>> GetWarframes(Dictionary<string, string> index)
    {
        string url = $"http://content.warframe.com/PublicExport/Manifest/${index["Warframes"]}";
        using HttpClient client = new HttpClient();
        var data = await client.GetStringAsync(url);

        JsonNode root = JsonNode.Parse(data)!;

        List<Item> warframes = new List<Item>();
        foreach (var item in root["ExportWarframes"]!.AsArray())
        {
            string name = item!["name"]!.ToString();
            string itemClass = ItemClasses[item["productCategory"]!.ToString()];
            string uniqueName = item["uniqueName"]!.ToString();
            string itemType = "";

            if (name.Contains("<ARCHWING> "))
            {
                name = name.Replace("<ARCHWING> ", "");
            }

            if (name.Contains("Prime"))
            {
                itemType = "Prime";
            }
            else if (name.Contains("Umbra"))
            {
                itemType = "Umbra";
            }
            else
            {
                itemType = "Normal";
            }


            warframes.Add(new Item
            {
                name = name,
                unique_name = uniqueName,
                item_class = itemClass,
                type = itemType,
            });
        }


        return warframes;
    }
}