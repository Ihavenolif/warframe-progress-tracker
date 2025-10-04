using rest_api.Services;

namespace rest_api_testing.ServiceTests;

public class WarframePublicExportServiceTest
{
    private readonly WarframePublicExportService _warframePublicExportService;

    /// <summary>
    /// .Ctor
    /// </summary>
    public WarframePublicExportServiceTest()
    {
        _warframePublicExportService = new WarframePublicExportService();
    }

    [Fact]
    public async Task GetIndex()
    {
        var result = await _warframePublicExportService.GetIndex();
        
        foreach (var (key, value) in result)
        {
            string innerKey;
            if (value.Contains("Manifest"))
            {
                innerKey = value.Split('.')[0][6..]; // Remove "Export" prefix
                Assert.Equal(innerKey, key);
                continue;
            }

            innerKey = value.Split('_')[0][6..];
            Assert.Equal(innerKey, key);
        }
    }
}