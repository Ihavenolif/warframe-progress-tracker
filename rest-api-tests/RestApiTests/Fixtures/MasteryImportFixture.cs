using System.Text.Json;

namespace rest_api_testing.Fixtures;

internal static class MasteryImportFixture
{
    public const string WeaponUniqueName = "/Items/TestWeapon";
    public const string WarframeUniqueName = "/Items/TestWarframe";
    public const string StaleUniqueName = "/Items/Stale";
    public const string MissionUniqueName = "/Missions/TestMission";
    public const string SecondMissionUniqueName = "/Missions/SecondMission";

    public static string FirstImport => CreateSnapshot(
        weaponXp: 500,
        warframeXp: 1000,
        missionTier: 0,
        duviriSkills: [1, 0, 0, 0],
        railjackSkills: [1, 0, 0, 0, 0]);

    public static string RepeatedImport => FirstImport;

    public static string ProgressiveImport => CreateSnapshot(
        weaponXp: 4500,
        warframeXp: 9000,
        missionTier: 1,
        duviriSkills: [2, 0, 0, 0],
        railjackSkills: [2, 0, 0, 0, 0]);

    public static string UnchangedImport => ProgressiveImport;

    public static string CorrectedImport => CreateSnapshot(
        weaponXp: 500,
        warframeXp: 0,
        missionTier: 0,
        duviriSkills: [0, 0, 0, 0],
        railjackSkills: [0, 0, 0, 0, 0]);

    public static string FullImport => CreateSnapshot(
        weaponXp: 450000,
        warframeXp: 900000,
        missionTier: 1,
        duviriSkills: [1, 2, 3, 4],
        railjackSkills: [1, 2, 3, 4, 5],
        includeSecondMission: true);

    private static string CreateSnapshot(
        int weaponXp,
        int warframeXp,
        int missionTier,
        int[] duviriSkills,
        int[] railjackSkills,
        bool includeSecondMission = false)
    {
        var missions = new List<object>
        {
            new { Tag = MissionUniqueName, Completes = 1, Tier = missionTier }
        };
        if (includeSecondMission)
        {
            missions.Add(new { Tag = SecondMissionUniqueName, Completes = 1, Tier = 0 });
        }

        return JsonSerializer.Serialize(new
        {
            XPInfo = new[]
            {
                new { ItemType = WeaponUniqueName, XP = weaponXp },
                new { ItemType = WarframeUniqueName, XP = warframeXp }
            },
            Recipes = Array.Empty<object>(),
            MiscItems = Array.Empty<object>(),
            PlayerLevel = 20,
            PlayerSkills = new Dictionary<string, int>
            {
                ["LPS_DRIFT_RIDING"] = duviriSkills[0],
                ["LPS_DRIFT_COMBAT"] = duviriSkills[1],
                ["LPS_DRIFT_OPPORTUNITY"] = duviriSkills[2],
                ["LPS_DRIFT_ENDURANCE"] = duviriSkills[3],
                ["LPS_PILOTING"] = railjackSkills[0],
                ["LPS_TACTICAL"] = railjackSkills[1],
                ["LPS_GUNNERY"] = railjackSkills[2],
                ["LPS_ENGINEERING"] = railjackSkills[3],
                ["LPS_COMMAND"] = railjackSkills[4]
            },
            Missions = missions
        });
    }
}
