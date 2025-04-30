import lzma
import requests
import json
import subprocess

REGISTERED: list[str] = []

ITEM_CLASSES = {
    "LongGuns": "Primary",
    "Pistols": "Secondary",
    "Melee": "Melee",
    "Suits": "Warframe",
    "Kdrive": "K-Drive",
    "MechSuits": "Necramech",
    "SpaceSuits": "Archwing",
    "Amp": "Amp",
    "Kitgun": "Kitgun",
    "OperatorAmps": "Amp",
    "Zaw": "Zaw",
    "SpaceMelee": "Archmelee",
    "Hound": "Hound",
    "KubrowPets": "Pet",
    "Moa": "Moa",
    "Sentinels": "Sentinel",
    "SentinelWeapons": "Sentinel Weapon",
    "SpaceGuns": "Archgun"
}


def decompress(string: str) -> str:
    return lzma.decompress(bytes(string, "latin1")).decode("utf-8")


def get_index() -> dict[str, str]:
    #fucking lzma is broken on ubuntu and i have to use this fucking shit
    req = requests.get(
       "https://origin.warframe.com/PublicExport/index_en.txt.lzma")
    decompressed = decompress(req.text)
    #fuckthis
    #TODO: once lzma is unfucked delete this shit
    #subprocess.run(["rm", "index_en.txt.lzma", "index_en.txt"])
    #subprocess.run(["wget", "https://origin.warframe.com/PublicExport/index_en.txt.lzma"])
    #subprocess.run(["node", "main.js"], stdout=open("index_en.txt", "w"))
#
    #with open("index_en.txt", "r") as fuckingfile:
    #    decompressed = fuckingfile.read()
    #    fuckingfile.close()

    ret = {}

    lines = decompressed.split("\r\n")
    for line in lines:
        # this is some python magic right fucking there
        line.replace("\r", "")
        key = line.split("_")[0][6::]
        ret[key] = line

    return ret


def get_warframes(index: dict[str, str], warframes: list) -> None:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Warframes']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    for warframe in parsed["ExportWarframes"]:
        name: str = warframe["name"]
        _class: str = ITEM_CLASSES[warframe["productCategory"]]
        _type: str = ""

        if "<ARCHWING> " in name:
            name = name.replace("<ARCHWING> ", "")

        if "Prime" in name:
            _type = "Prime"
        elif "Umbra" in name:
            _type = "Umbra"
        else:
            _type = "Normal"

        REGISTERED.append(name)
        warframes.append({
            "name": name,
            "class": _class,
            "type": _type,
            "nameraw": warframe["uniqueName"]
        })


def parse_amp(amp: dict, weapons: list) -> None:
    name: str = amp["name"]

    if "Prism" not in name:
        return

    _class: str = "Amp"
    _type: str = "Normal"

    REGISTERED.append(name)
    weapons.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": amp["uniqueName"]
    })


def parse_zaw(zaw: dict, weapons: list) -> None:
    name: str = zaw["name"]

    if "Tip" not in zaw["uniqueName"] or name in REGISTERED or "PvP" in zaw["uniqueName"]:
        return

    _class: str = "Zaw"
    _type: str = "Normal"

    REGISTERED.append(name)
    weapons.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": zaw["uniqueName"]
    })


def parse_kitgun(kitgun: dict, weapons: list) -> None:
    name: str = kitgun["name"]

    if "Barrel" not in kitgun["uniqueName"] or name in REGISTERED:
        return

    _class: str = "Kitgun"
    _type: str = "Normal"

    REGISTERED.append(name)
    weapons.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": kitgun["uniqueName"]
    })


def parse_hound(hound: dict, companions: list) -> None:
    name: str = hound["name"]

    if "Hound" not in hound["name"]:
        return

    _class: str = "Hound"
    _type: str = "Normal"

    REGISTERED.append(name)
    companions.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": hound["uniqueName"]
    })


def parse_moa(moa: dict, companions: list) -> None:
    name: str = moa["name"]

    if "Moa" not in moa["name"]:
        return

    _class: str = "Moa"
    _type: str = "Normal"

    REGISTERED.append(name)
    companions.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": moa["uniqueName"]
    })


def parse_kdrive(kdrive: dict, kdrives: list) -> None:
    name: str = kdrive["name"]
    uniqueName: str = kdrive["uniqueName"]

    if not uniqueName.endswith("Deck"):
        return

    _class: str = "Kdrive"
    _type: str = "Normal"

    REGISTERED.append(name)
    kdrives.append({
        "name": name,
        "class": _class,
        "type": _type,
        "nameraw": kdrive["uniqueName"]
    })


def get_weapons(index: dict[str, str], warframes: list, weapons: list, companions: list) -> None:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Weapons']}")
    parsed: dict = json.loads(req.text.replace("\r", "").replace("\n", ""))

    for item in parsed["ExportWeapons"]:
        if item["name"] in REGISTERED:
            continue

        if "SpecialItems" == item["productCategory"]:
            continue
        if "ANTIGEN" in item["name"] or "MUTAGEN" in item["name"]:
            continue
        if "Predasite" in item["name"] or "Vulpaphyla" in item["name"]:
            continue

        if "OperatorAmplifiers" in item["uniqueName"]:
            parse_amp(item, weapons)
            continue
        if "Ostron" in item["uniqueName"]:
            parse_zaw(item, weapons)
            continue
        if "InfKitGun" in item["uniqueName"] or "SolarisUnited" in item["uniqueName"]:
            parse_kitgun(item, weapons)
            continue
        if "MoaPets" in item["uniqueName"]:
            parse_moa(item, companions)
            continue
        if "ZanukaPets" in item["uniqueName"]:
            parse_hound(item, companions)
            continue
        if "Hoverboard" in item["uniqueName"]:
            parse_kdrive(item, warframes)
            continue

        name: str = item["name"]
        _class: str = ITEM_CLASSES[item["productCategory"]]
        _type: str = ""

        if "<ARCHWING> " in name:
            name = name.replace("<ARCHWING> ", "")

        if "Prime" in name:
            _type = "Prime"
        elif "Umbra" in name:
            _type = "Umbra"
        elif "Wraith" in name:
            _type = "Wraith"
        elif "Prisma" in name:
            _type = "Prisma"
        elif "Kuva" in name:
            _type = "Kuva"
        elif "Tenet" in name:
            _type = "Tenet"
        elif "Vandal" in name:
            _type = "Vandal"
        else:
            _type = "Normal"

        if item["productCategory"] == "SentinelWeapons":
            REGISTERED.append(name)
            companions.append({
                "name": name,
                "class": _class,
                "type": _type,
                "nameraw": item["uniqueName"]
            })
        else:
            REGISTERED.append(name)
            weapons.append({
                "name": name,
                "class": _class,
                "type": _type,
                "nameraw": item["uniqueName"]
            })
    pass


def get_sentinels(index: dict[str, str], companions: list) -> None:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Sentinels']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    for companion in parsed["ExportSentinels"]:

        name: str = companion["name"]

        _type: str = ""

        if "SpecialItems" == companion["productCategory"]:
            continue

        if "<ARCHWING> " in name:
            name = name.replace("<ARCHWING> ", "")

        if "Prime" in name:
            _type = "Prime"
        elif "Umbra" in name:
            _type = "Umbra"
        elif "Wraith" in name:
            _type = "Wraith"
        elif "Prisma" in name:
            _type = "Prisma"
        elif "Kuva" in name:
            _type = "Kuva"
        elif "Tenet" in name:
            _type = "Tenet"
        elif "Vandal" in name:
            _type = "Vandal"
        else:
            _type = "Normal"

        _class: str = ITEM_CLASSES[companion["productCategory"]]

        REGISTERED.append(name)
        companions.append({
            "name": name,
            "class": _class,
            "type": _type,
            "nameraw": companion["uniqueName"]
        })

def get_recipes(index: dict[str, str], gear_item_names: list[str]) -> list:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Recipes']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    recipes = []

    for recipe in parsed["ExportRecipes"]:
        if not str(recipe["resultType"]) in gear_item_names: continue
        recipes.append(recipe)
    
    # Now we have recipes for all of the gear items
    # We also need recipes for all of the weapon components

    gear_recipe_ingredients = list(map(lambda x: x["ingredients"], recipes))
    gear_recipe_ingredients = list(item for sublist in gear_recipe_ingredients for item in sublist)
    gear_recipe_ingredients = list(set(map(lambda x: x["ItemType"], gear_recipe_ingredients)))

    for recipe in parsed["ExportRecipes"]:
        if not str(recipe["resultType"]) in gear_recipe_ingredients: continue
        recipes.append(recipe)

    return recipes

def get_resources(index: dict[str, str]) -> list:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Resources']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    return parsed["ExportResources"]

def main():
    index = get_index()

    warframes = []
    weapons = []
    companions = []

    get_warframes(index, warframes)
    get_weapons(index, warframes, weapons, companions)
    get_sentinels(index, companions)
    pass


if __name__ == "__main__":
    main()
