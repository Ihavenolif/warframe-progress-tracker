import copy
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from data import normalize_store_item_unique_name, parse_relics


def relic_variant(suffix, rewards=None, internal_name="TestPrimeA"):
    return {
        "uniqueName": f"/Lotus/Types/Game/Projections/T1VoidProjection{internal_name}{suffix}",
        "name": "Lith T1 Relic",
        "relicRewards": rewards or [
            {
                "rewardName": "/Lotus/StoreItems/Types/Recipes/Weapons/TestPrimeBlueprint",
                "rarity": "RARE",
                "itemCount": 1
            },
            {
                "rewardName": "/Lotus/StoreItems/Types/Recipes/Weapons/WeaponParts/TestPrimeBarrel",
                "rarity": "COMMON",
                "itemCount": 1
            },
            {
                "rewardName": "/Lotus/StoreItems/Types/Recipes/Components/FormaBlueprint",
                "rarity": "COMMON",
                "itemCount": 1
            }
        ]
    }


class RelicParserTests(unittest.TestCase):
    def setUp(self):
        self.records = [
            relic_variant("Bronze"),
            relic_variant("Silver"),
            relic_variant("Gold"),
            relic_variant("Platinum")
        ]

    def test_normalizes_store_item_path(self):
        self.assertEqual(
            "/Lotus/Types/Recipes/Weapons/TestPrimeBlueprint",
            normalize_store_item_unique_name(
                "/Lotus/StoreItems/Types/Recipes/Weapons/TestPrimeBlueprint"
            )
        )

    def test_groups_refinements_and_internal_aliases(self):
        self.records.append(relic_variant("Bronze", internal_name="TestPrimeB"))
        self.records.append({
            "uniqueName": "/Lotus/Types/Game/Projections/T5VoidProjectionImmortalOmniABronze",
            "name": "Requiem Eterna Relic",
            "relicRewards": []
        })
        self.records.append({
            "uniqueName": "/Lotus/Types/Game/Projections/FakeBronze",
            "name": "Lithium T1 Relic",
            "relicRewards": []
        })

        parsed = parse_relics(self.records)

        self.assertEqual([{"name": "Lith T1", "era": "Lith"}], parsed["relics"])
        self.assertEqual(5, len(parsed["variants"]))
        self.assertEqual(
            {"Intact", "Exceptional", "Flawless", "Radiant"},
            {variant["refinement"] for variant in parsed["variants"]}
        )
        self.assertEqual(2, len(parsed["rewards"]))
        self.assertIn(
            "/Lotus/Types/Recipes/Weapons/TestPrimeBlueprint",
            {reward["reward_unique_name"] for reward in parsed["rewards"]}
        )

    def test_rejects_inconsistent_refinement_rewards(self):
        changed_rewards = copy.deepcopy(self.records[0]["relicRewards"])
        changed_rewards[0]["rarity"] = "COMMON"
        self.records[1]["relicRewards"] = changed_rewards

        with self.assertRaisesRegex(ValueError, "Refinement reward mismatch for Lith T1"):
            parse_relics(self.records)

    def test_rejects_missing_refinement(self):
        with self.assertRaisesRegex(ValueError, "Expected all refinements for Lith T1"):
            parse_relics(self.records[:-1])


if __name__ == "__main__":
    unittest.main()
