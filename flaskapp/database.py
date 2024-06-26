from flask_login import UserMixin, login_user, LoginManager, login_required, logout_user, current_user
from flask_sqlalchemy import SQLAlchemy
from config import load_config

from flask_app import app

db = SQLAlchemy(app)
