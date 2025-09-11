import psycopg2
from pg_config import load_config, load_env
from data import *


def connect(config) -> psycopg2.extensions.connection:
    """ Connect to the PostgreSQL database server """
    try:
        # connecting to the PostgreSQL server
        with psycopg2.connect(**config) as conn:
            print('Connected to the PostgreSQL server.')
            return conn
    except (psycopg2.DatabaseError, Exception) as error:
        print(error)


def send_data():
    warframes = []
    weapons = []
    companions = []

    index = get_index()
    get_weapons(index, warframes, weapons, companions)
    get_warframes(index, warframes)
    get_sentinels(index, companions)

    gear_items = warframes + weapons + companions

    gear_item_names = list(map(lambda x: x["nameraw"], gear_items))

    recipes = get_recipes(index, gear_item_names)
    resources = get_resources(index)

    config = load_env()

    gear_items_insert = []
    resources_insert = []
    recipe_items_insert = []
    recipes_insert = []
    recipes_ingredients_insert = []

    connection = connect(config)
    cursor = connection.cursor()

    for item in gear_items:
        if item["class"] == "Necramech":
            xp_required = 1600000
        elif "Kuva" in item["name"] or "Tenet" in item["name"] or "Coda" in item["name"] or item["name"] == "Paracesis":
            xp_required = 800000
        elif item["class"] in [
            "Amp",
            "Archgun",
            "Archmelee",
            "Kitgun",
            "Melee",
            "Primary",
            "Secondary",
            "Sentinel Weapon",
            "Zaw"
        ]:
            xp_required = 450000
        elif item["class"] in [
            "Archwing",
            "Hound",
            "Kdrive",
            "Moa",
            "Pet",
            "Sentinel",
            "Warframe"
        ]:
            xp_required = 900000
        else:
            raise Exception("nejaka picovina mi utekla")

        gear_items_insert.append(
            (item["name"], item["nameraw"], item["type"], item["class"], xp_required))

        # try:
        #     cursor.execute(f"INSERT INTO item (name, nameraw, type, item_class, xp_required) VALUES ('{item['name']}', '{item['nameraw']}', '{item['type']}', '{item['class']}', {xp_required});")
        # except:pass
        # cursor.execute(f"INSERT INTO warframe (name, item_class) VALUES ('{warframe['name']}', '{warframe['class']}')")

    for resource in resources:
        resources_insert.append(
            (resource["name"], resource["uniqueName"], "Resource", "MiscItems"))

        if "Lanthorn" in resource["name"]:
            pass
        # try:
        #     cursor.execute(f"INSERT INTO item (name, nameraw, type, item_class) VALUES ('{resource['name']}', '{resource['uniqueName']}', 'Resource', 'MiscItems');")
        # except Exception as e:
        #     print(e)
        #     print(resource)
        #     print(f"INSERT INTO item (name, nameraw, type, item_class) VALUES ('{resource['name']}', '{resource['uniqueName']}', 'Resource', 'MiscItems');")
        #     exit()

    for recipe in recipes:
        # if recipe["uniqueName"] in CONFLICTING_ITEM_NAMES:
        #    continue
        #    recipe["uniqueName"] = CONFLICTING_ITEM_NAMES[recipe["uniqueName"]]

        if recipe["uniqueName"] in ITEMS_TO_IGNORE:
            continue

        if recipe["uniqueName"] in ITEMS_TO_REPLACE:
            recipe["uniqueName"] = ITEMS_TO_REPLACE[recipe["uniqueName"]]

        if 'LowKatana' in recipe["uniqueName"] or 'LowKatana' in recipe["resultType"]:
            pass
        recipe_items_insert.append(
            (recipe["resultType"], recipe["uniqueName"], "Recipe", "MiscItems"))
        recipes_insert.append((recipe["uniqueName"], recipe["resultType"]))

        for ingredient in recipe["ingredients"]:
            recipes_ingredients_insert.append(
                (recipe["uniqueName"], ingredient["ItemType"], ingredient["ItemCount"]))

    cursor.executemany(
        "INSERT INTO item (name, unique_name, type, item_class, xp_required) VALUES (%s, %s, %s, %s, %s) ON CONFLICT (unique_name) DO NOTHING", gear_items_insert)
    cursor.executemany(
        "INSERT INTO item (name, unique_name, type, item_class) VALUES (%s, %s, %s, %s) ON CONFLICT (unique_name) DO NOTHING", resources_insert)
    connection.commit()
    cursor.executemany(
        "INSERT INTO item (name, unique_name, type, item_class) VALUES (%s, %s, %s, %s) ON CONFLICT (unique_name) DO NOTHING", recipe_items_insert)
    cursor.executemany(
        "INSERT INTO recipe (unique_name, result_item) VALUES (%s, %s) ON CONFLICT (unique_name) DO NOTHING", recipes_insert)
    cursor.executemany("INSERT INTO recipe_ingredients (recipe_name, item_ingredient, ingredient_count) VALUES (%s, %s, %s) ON CONFLICT (recipe_name, item_ingredient) DO NOTHING", recipes_ingredients_insert)

    connection.commit()
    connection.close()


def main():
    send_data()


if __name__ == '__main__':
    main()
