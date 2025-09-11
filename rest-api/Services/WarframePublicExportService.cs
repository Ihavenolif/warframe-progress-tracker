using SharpCompress.Compressors.LZMA;

namespace rest_api.Services;

public interface IWarframePublicExportService
{
    public Task<Dictionary<string, string>> GetIndex();
}

public class WarframePublicExportService : IWarframePublicExportService
{
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
}