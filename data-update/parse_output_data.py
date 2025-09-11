from pg_send_data import *

def import_data(player_id, filename):
    with open(filename, "r") as file:
        data = json.load(file)

    config = load_config()
    connection = connect(config)
    cursor = connection.cursor()

    cursor.execute("SELECT unique_name FROM recipe")
    recipes_present = list(map(lambda x: x[0], cursor.fetchall()))

    cursor.execute("SELECT unique_name FROM item")
    items_present = list(map(lambda x: x[0], cursor.fetchall()))

    mastery_items = list(map(lambda x: (x["ItemType"], player_id, x["XP"]), filter(lambda y: y["ItemType"] in items_present, data["XPInfo"])))
    recipes = list(map(lambda x: (x["ItemType"], player_id, x["ItemCount"]), filter(lambda y: y["ItemType"] in recipes_present, data["Recipes"])))
    misc_items = list(map(lambda x: (x["ItemType"], player_id, x["ItemCount"]), filter(lambda y: y["ItemType"] in items_present, data["MiscItems"])))

    cursor.executemany("INSERT INTO player_items_mastery (unique_name, player_id, xp_gained) VALUES (%s, %s, %s) ON CONFLICT (unique_name, player_id) DO UPDATE SET xp_gained=excluded.xp_gained", mastery_items)
    cursor.executemany("INSERT INTO player_items (unique_name, player_id, item_count) VALUES (%s, %s, %s) ON CONFLICT (unique_name, player_id) DO UPDATE SET item_count=excluded.item_count", recipes)
    cursor.executemany("INSERT INTO player_items (unique_name, player_id, item_count) VALUES (%s, %s, %s) ON CONFLICT (unique_name, player_id) DO UPDATE SET item_count=excluded.item_count", misc_items)

    connection.commit()

def main():
    import_data(1, "./out.json")
    import_data(2, "./out_lanaes.json")

    pass


if __name__ == '__main__':
    main()
