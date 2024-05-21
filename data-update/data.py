import lzma
import requests
import json

REGISTERED: list[str] = []


def decompress(string: str) -> str:
    return lzma.decompress(bytes(string, "latin1")).decode("utf-8")


def get_index() -> dict[str, str]:
    req = requests.get(
        "https://origin.warframe.com/PublicExport/index_en.txt.lzma")
    decompressed = decompress(req.text)
    ret = {}

    lines = decompressed.split("\r\n")
    for line in lines:
        # this is some python magic right fucking there
        key = line.split("_")[0][6::]
        ret[key] = line

    return ret


def get_warframes(index: dict[str, str], warframes: list) -> None:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Warframes']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    for warframe in parsed["ExportWarframes"]:
        name: str = warframe["name"]
        _class: str = warframe["productCategory"]
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
            "type": _type
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
        "type": _type
    })


def parse_zaw(zaw: dict, weapons: list) -> None:
    name: str = zaw["name"]

    if "Tip" not in zaw["uniqueName"] or name in REGISTERED:
        return

    _class: str = "Zaw"
    _type: str = "Normal"

    REGISTERED.append(name)
    weapons.append({
        "name": name,
        "class": _class,
        "type": _type
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
        "type": _type
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
        "type": _type
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
        "type": _type
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
        "type": _type
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
        _class: str = item["productCategory"]
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

        if _class == "SentinelWeapons":
            REGISTERED.append(name)
            companions.append({
                "name": name,
                "class": _class,
                "type": _type
            })
        else:
            REGISTERED.append(name)
            weapons.append({
                "name": name,
                "class": _class,
                "type": _type
            })
    pass


def get_sentinels(index: dict[str, str], companions: list) -> None:
    req = requests.get(
        f"http://content.warframe.com/PublicExport/Manifest/{index['Sentinels']}")
    parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))

    for companion in parsed["ExportSentinels"]:

        name: str = companion["name"]
        _class: str = companion["productCategory"]
        _type: str = ""

        if _class == "SpecialItems":
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

        REGISTERED.append(name)
        companions.append({
            "name": name,
            "class": _class,
            "type": _type
        })


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
