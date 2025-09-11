from data import *

index = get_index()

parsed = get_images(index)

print(index)


with open("test.json", "w") as recipes_json_file:
    recipes_json_file.write(json.dumps(parsed))
