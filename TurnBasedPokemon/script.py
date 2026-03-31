import json
from collections import OrderedDict

INPUT_FILE = "pokemon.json"
OUTPUT_FILE = "output.json"

def insert_baseexp(obj):

    if "CatchRate" in obj:
        return obj

    new_obj = OrderedDict()

    for key in obj:
        if key == "BaseExp":
            new_obj["CatchRate"] = 0

        new_obj[key] = obj[key]

    return new_obj


with open(INPUT_FILE, "r", encoding="utf-8") as f:
    data = json.load(f, object_pairs_hook=OrderedDict)

if isinstance(data, list):
    new_data = [insert_baseexp(obj) for obj in data]
else:
    new_data = insert_baseexp(data)

with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
    json.dump(new_data, f, indent=4, ensure_ascii=False)

print("Done!")