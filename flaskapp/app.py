from wtforms.validators import DataRequired, EqualTo
from wtforms import SubmitField, StringField, PasswordField
from flask_wtf import FlaskForm
from flask import Flask, render_template, render_template_string, redirect, url_for
from flask_login import UserMixin, login_user, LoginManager, login_required, logout_user, current_user
from flask_sqlalchemy import SQLAlchemy
from sqlalchemy.orm import Mapped, mapped_column

import flask_sqlalchemy.model
import flask_sqlalchemy.extension
import hashlib

from config import load_config
from util import render, generate_random_password

DB_CONFIG = load_config()

app = Flask(__name__)
app.config["SECRET_KEY"] = "KOKOT"
app.config["SQLALCHEMY_DATABASE_URI"] = f"postgresql://{DB_CONFIG["user"]}:{DB_CONFIG["password"]}@{DB_CONFIG["host"]}:5432/{DB_CONFIG["database"]}"

login_manager = LoginManager()
login_manager.init_app(app)
login_manager.login_view = "login"

db: flask_sqlalchemy.extension.SQLAlchemy = SQLAlchemy(app)


class RegisterForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired(), EqualTo("password_repeat", message="Passwords must match")])
    password_repeat = PasswordField("Repeat password", validators=[DataRequired()])


class LoginForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired()])


class Registered_user(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(256), unique=True)
    password_hash = db.Column(db.String(256))
    salt = db.Column(db.String(256))


@login_manager.user_loader
def load_user(user_id):
    return Registered_user.query.get(int(user_id))


@app.route("/")
def index():
    return render("index.html")


@app.route("/login", methods=["GET", "POST"])
def login():
    form: LoginForm = LoginForm()
    if not form.validate_on_submit():
        return render("login.html", form=form)

    username = form.username.data
    password = form.password.data

    user: Registered_user = Registered_user.query.filter_by(username=username).first()

    if not user:
        # user not found
        return render("login.html", form=form)

    password_hash = hashlib.sha256((password+user.salt).encode("utf-8")).hexdigest()
    if user.password_hash == password_hash:
        login_user(user)
    else:
        # incorrect password
        return render("login.html", form=form)
    return redirect("/")


@app.route("/register", methods=["GET", "POST"])
def register():
    form: RegisterForm = RegisterForm()

    if not form.validate_on_submit():
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        return render("register.html", form=form)

    username: str = form.username.data

    if Registered_user.query.filter_by(username=username).first():
        return "User already exists"

    password: str = form.password.data
    salt = generate_random_password(32)
    password_hash = hashlib.sha256((password+salt).encode("utf-8")).hexdigest()

    user: Registered_user = Registered_user(username=username, password_hash=password_hash, salt=salt)
    db.session.add(user)
    db.session.commit()
    logout_user()
    login_user(user)
    return "Registered with username " + username


@login_required
@app.route("/logout")
def logout():
    logout_user()
    return render("logout.html")


if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)
