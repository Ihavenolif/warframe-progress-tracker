import importlib
import os

from config import load_config

from flask_app import app, login_manager
from models.registered_user import Registered_user

DB_CONFIG = load_config()


@login_manager.user_loader
def load_user(user_id):
    return Registered_user.query.get(int(user_id))


def import_routes():
    def recursive_import(package_name, directory):
        for filename in os.listdir(directory):
            filepath = os.path.join(directory, filename)
            if os.path.isdir(filepath):
                recursive_import(f"{package_name}.{filename}", filepath)
            elif filename.endswith('.py') and filename != '__init__.py':
                module_name = f"{package_name}.{filename[:-3]}"
                importlib.import_module(module_name)

    routes_dir = os.path.join(os.path.dirname(__file__), 'routes')
    recursive_import('routes', routes_dir)


if __name__ == "__main__":
    import_routes()
    app.run(host="127.0.0.1", port=5000, debug=True)
