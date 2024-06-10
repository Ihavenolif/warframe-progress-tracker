from flask import Flask
from flask_login import LoginManager

from config import load_config

DB_CONFIG = load_config()

app = Flask(__name__)
app.config["SECRET_KEY"] = "KOKOT"
app.config["SQLALCHEMY_DATABASE_URI"] = f"postgresql://{DB_CONFIG["user"]}:{DB_CONFIG["password"]}@{DB_CONFIG["host"]}:5432/{DB_CONFIG["database"]}"
app.config["UPLOAD_FOLDER"] = "temp"

login_manager = LoginManager()
login_manager.init_app(app)
login_manager.login_view = "login"
