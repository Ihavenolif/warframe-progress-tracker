using System.Text;
using Decoder = SharpCompress.Compressors.LZMA.Decoder;

namespace rest_api.Services;

public interface IWarframePublicExportService
{
    public Task<Dictionary<string, string>> GetIndex();
}

public class WarframePublicExportService : IWarframePublicExportService
{
    private const string RAW_INDEX_URL = "https://origin.warframe.com/PublicExport/index_en.txt.lzma";
    
    private const string SPLIT_RAW_INDEX_BY = "\r\n";
    private const char MANIFEST_TILL_CHAR = '.';
    private const char NON_MANIFEST_TILL_CHAR = '_';
    
    private static async Task<string> GetRawIndex()
    {
        using var client = new HttpClient();
        var compressedData = await client.GetByteArrayAsync(RAW_INDEX_URL);

        using var input = new MemoryStream(compressedData);
        using var output = new MemoryStream();

        // Read the first 5 bytes: properties
        var properties = new byte[5];
        input.Read(properties, 0, 5);

        // Read the next 8 bytes: uncompressed size (little endian)
        Span<byte> sizeBytes = stackalloc byte[8];
        input.Read(sizeBytes);
        var outSize = BitConverter.ToInt64(sizeBytes);

        // Decode
        var decoder = new Decoder();
        decoder.SetDecoderProperties(properties);
        decoder.Code(input, output, input.Length - input.Position, outSize, null);
        
        return Encoding.UTF8.GetString(output.ToArray());
    }
    
    public async Task<Dictionary<string, string>> GetIndex()
    {
        var rawIndex = await GetRawIndex();
        var result = new Dictionary<string, string>();

        var lines = rawIndex.Split(SPLIT_RAW_INDEX_BY);
        foreach (var line in lines)
        {
            var charToSplit = line.Contains("Manifest") ? MANIFEST_TILL_CHAR : NON_MANIFEST_TILL_CHAR;
            var key = TakeTillAndSkip(line, charToSplit, 6);
            result[key] = line;
        }

        return result;

        static string TakeTillAndSkip(string value, char till, int startingIndex)
        {
            var sb = new StringBuilder(); // StringBuilderPool can better option here.
            
            for (var i = startingIndex; i < value.Length; i++)
            {
                var @char = value[i]; 
                if (@char == till) break;
                sb.Append(@char);
            }
            
            var result = sb.ToString();
            return result;
        }
    }
}