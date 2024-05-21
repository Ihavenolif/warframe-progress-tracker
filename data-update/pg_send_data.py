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
    config = load_config()

    connection = connect(config)
    cursor = connection.cursor()

    warframes = []
    weapons = []
    companions = []

    index = get_index()
    get_weapons(index, warframes, weapons, companions)
    get_warframes(index, warframes)
    get_sentinels(index, companions)

    test_warframe = warframes[0]

    cursor.execute(f"INSERT INTO item (name, type) VALUES ({test_warframe["name"]}, {test_warframe["type"]});")
    cursor.execute(f"INSERT INTO warframe (name, class) VALUES ({test_warframe["name"], test_warframe["class"]})")

    connection.commit()


if __name__ == '__main__':
    main()
