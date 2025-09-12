using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Player_items_mastery
{
    public string unique_name { get; set; } = null!;

    public int player_id { get; set; }

    public int xp_gained { get; set; }

    public virtual Player player { get; set; } = null!;

    public virtual Item item { get; set; } = null!;

    public readonly int[] WarframeThresholds = [
        0, 1000, 4000, 9000, 16000, 25000, 36000, 49000, 64000, 81000,
        100000, 121000, 144000, 169000, 196000, 225000, 256000, 289000,
        324000, 361000, 400000, 441000, 484000, 529000, 576000, 625000,
        676000, 729000, 784000, 841000, 900000, 961000, 1024000, 1089000,
        1156000, 1225000, 1296000, 1369000, 1444000, 1521000, 1600000
    ];

    public readonly int[] WeaponThresholds =
    [
        0, 500, 2000, 4500, 8000, 12500, 18000, 24500, 32000, 40500,
        50000, 60500, 72000, 84500, 98000, 112500, 128000, 144500,
        162000, 180500, 200000, 220500, 242000, 264500, 288000, 312500,
        338000, 364500, 392000, 420500, 450000, 480500, 512000, 544500,
        578000, 612500, 648000, 684500, 722000, 760500, 800000
    ];

    public int GetRank()
    {
        int[] thresholds = item?.xp_required == 1600000 || item?.xp_required == 900000 ? WarframeThresholds :
            item?.xp_required == 800000 || item?.xp_required == 450000 ? WeaponThresholds :
            throw new InvalidOperationException("Unknown item xp_required value");

        int maxLevel = item?.xp_required == 1600000 || item?.xp_required == 800000 ? 40 : 30;

        for (int level = 1; level < maxLevel + 1; level++)
        {
            if (xp_gained < thresholds[level])
                return level - 1;
        }

        // Max rank reached
        return maxLevel;
    }

    public int GetMasteryPoints()
    {
        int rank = GetRank();
        if (rank == 0) return 0;
        int res = item?.xp_required == 1600000 || item?.xp_required == 900000 ? rank * 200 :
               item?.xp_required == 800000 || item?.xp_required == 450000 ? rank * 100 :
               throw new InvalidOperationException("Unknown item xp_required value");

        if (res == 3000)
        {
            Console.WriteLine("Something is fishy");
        }

        return res;
    }
}
