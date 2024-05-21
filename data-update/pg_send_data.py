import psycopg2
from pg_config import load_config


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

    cursor.execute("select * from clan;")
    print(cursor.fetchall())


if __name__ == '__main__':
    main()
