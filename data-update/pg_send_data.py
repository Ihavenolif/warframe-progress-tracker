import psycopg2
from pg_config import load_config
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


def main():
    warframes = []
    weapons = []
    companions = []

    index = get_index()
    get_weapons(index, warframes, weapons, companions)
    get_warframes(index, warframes)
    get_sentinels(index, companions)

    config = load_config()

    connection = connect(config)
    cursor = connection.cursor()

    for warframe in warframes:
        cursor.execute(f"INSERT INTO item (name, nameraw, type) VALUES ('{warframe['name']}', '{warframe['nameraw']}', '{warframe['type']}');")
        cursor.execute(f"INSERT INTO warframe (name, item_class) VALUES ('{warframe['name']}', '{warframe['class']}')")

    for weapon in weapons:
        cursor.execute(f"INSERT INTO item (name, nameraw, type) VALUES ('{weapon['name']}', '{weapon['nameraw']}', '{weapon['type']}');")
        cursor.execute(f"INSERT INTO weapon (name, item_class) VALUES ('{weapon['name']}', '{weapon['class']}')")

    for companion in companions:
        cursor.execute(f"INSERT INTO item (name, nameraw,type) VALUES ('{companion['name']}', '{companion['nameraw']}', '{companion['type']}');")
        cursor.execute(f"INSERT INTO companion (name, item_class) VALUES ('{companion['name']}', '{companion['class']}')")

    connection.commit()


if __name__ == '__main__':
    main()
