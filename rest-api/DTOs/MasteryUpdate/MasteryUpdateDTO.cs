namespace rest_api.DTO.MasteryUpdate;

public class MasteryUpdateDTO{
    public IEnumerable<XPInfoItem> XPInfo { get; set; } = new List<XPInfoItem>();
    public IEnumerable<MiscItem> MiscItems { get; set; } = new List<MiscItem>();
    public IEnumerable<MiscItem> Recipes { get; set; } = new List<MiscItem>();
}