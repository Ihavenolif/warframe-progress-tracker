from data import *

index = get_index()

warframes = []
weapons = []
companions = []

get_warframes(index, warframes)
get_weapons(index, warframes, weapons, companions)
get_sentinels(index, companions)

all_items = warframes + companions + weapons
gear_item_names = list(map(lambda x: x["nameraw"], all_items))

recipes = get_recipes(index, gear_item_names)

with open("recipes_filtered.json", "w") as recipes_json_file:
    recipes_json_file.write(json.dumps(list(recipes)))