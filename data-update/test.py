from data import *

index = get_index()

req = requests.get(
    f"http://content.warframe.com/PublicExport/Manifest/{index['Resources']}")
parsed = json.loads(req.text.replace("\r", "").replace("\n", ""))


with open("test.json", "w") as recipes_json_file:
    recipes_json_file.write(json.dumps(parsed))