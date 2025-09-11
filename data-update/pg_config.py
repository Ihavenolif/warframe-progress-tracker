from configparser import ConfigParser
from dotenv import load_dotenv
import os


def load_config(filename='../database.ini', section='postgresql'):
    parser = ConfigParser()
    parser.read(filename)

    # get section, default to postgresql
    config = {}
    if parser.has_section(section):
        params = parser.items(section)
        for param in params:
            config[param[0]] = param[1]
    else:
        raise Exception(
            'Section {0} not found in the {1} file'.format(section, filename))

    return config


def load_env():
    load_dotenv("../.env")
    host = os.getenv("DB_HOST")
    database = os.getenv("POSTGRES_DB")
    user = os.getenv("POSTGRES_USER")
    password = os.getenv("POSTGRES_PASSWORD")
    return {
        "host": host,
        "database": database,
        "user": user,
        "password": password
    }


if __name__ == '__main__':
    config = load_config()
    print(config)
