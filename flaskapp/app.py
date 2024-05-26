from wtforms.validators import DataRequired, EqualTo
from wtforms import SubmitField, StringField, PasswordField
from flask_wtf import FlaskForm
from flask_wtf.file import FileField, FileRequired, FileStorage
from flask import Flask, redirect, flash, request, render_template
from flask_login import UserMixin, login_user, LoginManager, login_required, logout_user, current_user
from flask_sqlalchemy import SQLAlchemy
from sqlalchemy import Row, Tuple
from sqlalchemy.orm import Mapped, mapped_column, aliased
from sqlalchemy import desc, text, select, func
from werkzeug.utils import secure_filename

import flask_sqlalchemy.model
import flask_sqlalchemy.extension
import hashlib
import json
import time
import os

from config import load_config
from util import render, generate_random_password, ITEM_CLASSES

DB_CONFIG = load_config()

app = Flask(__name__)
app.config["SECRET_KEY"] = "KOKOT"
app.config["SQLALCHEMY_DATABASE_URI"] = f"postgresql://{DB_CONFIG["user"]}:{DB_CONFIG["password"]}@{DB_CONFIG["host"]}:5432/{DB_CONFIG["database"]}"
app.config["UPLOAD_FOLDER"] = "temp"

login_manager = LoginManager()
login_manager.init_app(app)
login_manager.login_view = "login"

db: flask_sqlalchemy.extension.SQLAlchemy = SQLAlchemy(app)


class RegisterForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired(), EqualTo("password_repeat", message="Passwords must match")])
    password_repeat = PasswordField("Repeat password", validators=[DataRequired()])


class ChangePasswordForm(FlaskForm):
    old_password = PasswordField("Old password", validators=[DataRequired()])
    new_password = PasswordField("New password", validators=[DataRequired(), EqualTo("password_repeat", message="Passwords must match")])
    password_repeat = PasswordField("Repeat password", validators=[DataRequired()])


class LoginForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired()])


class LinkAccountForm(FlaskForm):
    warframe_name = StringField("Account name", validators=[DataRequired()])


class UploadFileForm(FlaskForm):
    file = FileField("File")
    submit = SubmitField("Upload File")


class Registered_user(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(256), unique=True)
    password_hash = db.Column(db.String(256))
    salt = db.Column(db.String(256))


class Player(db.Model):
    username = db.Column(db.String(256))
    id = db.Column(db.Integer, primary_key=True)
    registered_user_id = db.Column(db.Integer)
    mastery_rank = db.Column(db.Integer)


class Item(db.Model):
    name = db.Column(db.String(256), primary_key=True)
    nameraw = db.Column(db.String(256))
    type = db.Column(db.String(256))


class PlayerItems(db.Model):
    player_id = db.Column(db.Integer, primary_key=True)
    item_name = db.Column(db.String(256), primary_key=True)
    state = db.Column(db.Integer)


class Warframe(db.Model):
    name = db.Column(db.String(256), primary_key=True)
    item_class = db.Column(db.String(256))


class Weapon(db.Model):
    name = db.Column(db.String(256), primary_key=True)
    item_class = db.Column(db.String(256))


class Companion(db.Model):
    name = db.Column(db.String(256), primary_key=True)
    item_class = db.Column(db.String(256))


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
        flash("No user with that username exists.", "error")
        return render("login.html", form=form)

    password_hash = hashlib.sha256((password+user.salt).encode("utf-8")).hexdigest()
    if user.password_hash == password_hash:
        login_user(user)
    else:
        # incorrect password
        flash("Incorrect password.", "error")
        return render("login.html", form=form)
    flash("Successfully logged in.", "info")
    return redirect("/")


@app.route("/register", methods=["GET", "POST"])
def register():
    form: RegisterForm = RegisterForm()

    if not form.validate_on_submit():
        try:
            error = form.errors["password"][0]
            flash(error, "error")
        except:
            pass
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        return render("register.html", form=form)

    username: str = form.username.data

    if Registered_user.query.filter_by(username=username).first():
        flash("That username is already taken.", "error")
        return render("register.html", form=form)

    password: str = form.password.data
    salt = generate_random_password(32)
    password_hash = hashlib.sha256((password+salt).encode("utf-8")).hexdigest()

    user: Registered_user = Registered_user(username=username, password_hash=password_hash, salt=salt)
    db.session.add(user)
    db.session.commit()
    logout_user()
    login_user(user)
    flash("Registered with username " + username + ".", "info")
    return redirect("/")


@login_required
@app.route("/logout")
def logout():
    logout_user()
    flash("Successfully logged out.", "info")
    return redirect("/")


@login_required
@app.route("/clans")
def clans():
    pass


@login_required
@app.route("/settings")
def settings():
    player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()
    if player:
        warframe_name = player.username
    else:
        warframe_name = ""
    return render("settings.html", warframe_name=warframe_name)
    pass


@login_required
@app.route("/change_password", methods=["GET", "POST"])
def change_password():
    form: ChangePasswordForm = ChangePasswordForm()

    if not form.validate_on_submit():
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        try:
            error = form.errors["password"][0]
            flash(error, "error")
        except:
            pass
        return render("change_password.html", form=form)

    old_password: str = form.old_password.data
    salt = getattr(current_user, "salt")
    old_password_hash = hashlib.sha256((old_password+salt).encode("utf-8")).hexdigest()

    if old_password_hash != getattr(current_user, "password_hash"):
        flash("Old password is wrong.", "error")
        return render("change_password.html", form=form)

    new_password: str = form.new_password.data
    new_password_hash = hashlib.sha256((new_password+salt).encode("utf-8")).hexdigest()

    current_user.password_hash = new_password_hash
    db.session.commit()
    flash("Password changed successfully.", "info")
    return redirect("/settings")


@login_required
@app.route("/link_account", methods=["GET", "POST"])
def link_account():
    form: LinkAccountForm = LinkAccountForm()

    player_id = getattr(current_user, "id")
    potentially_linked_account = Player.query.filter_by(registered_user_id=player_id).first()
    if potentially_linked_account:
        flash("A Warframe account is already linked.", "error")
        return redirect("/settings")

    if not form.validate_on_submit():
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        try:
            error = form.errors["password"][0]
            flash(error, "error")
        except:
            pass
        return render("link_account.html", form=form)

    warframe_name: str = form.warframe_name.data
    player = Player(username=warframe_name, registered_user_id=player_id, mastery_rank=0)
    db.session.add(player)
    db.session.commit()
    flash("Account successfully linked.", "info")
    return redirect("/settings")


@login_required
@app.route("/unlink_account")
def unlink_account():
    player_id = getattr(current_user, "id")
    player = Player.query.filter_by(registered_user_id=player_id)

    if not player:
        flash("No account is currently linked.", "error")
        return redirect("/settings")

    player.delete()

    db.session.commit()
    flash("Account unlinked successfully.", "info")
    return redirect("/settings")


@login_required
@app.route("/progress", methods=["GET", "POST"])
def progress():
    if request.content_type == "application/json":
        sort_type_dictionary = {
            "state": PlayerItems.state,
            "name": Item.name,
            "class": Weapon.item_class
        }
        sortJson = request.json["sorting"]
        sortOrder: list = []

        for sort in sortJson:
            if sort["ascending"]:
                sortOrder.append(sort_type_dictionary[sort["type"]])
            else:
                sortOrder.append(desc(sort_type_dictionary[sort["type"]]))

        filters = request.json["filters"]

        print(request.json)
    else:
        sortOrder: list = [
            PlayerItems.state,
            Weapon.item_class,
            Item.name
        ]

        filters = {
            "state": "all",
            "type": "",
            "name": ""
        }
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    weaponQuery = db.session.query(PlayerItems, Item, Weapon)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Weapon, Weapon.name == Item.name)
    warframeQuery = db.session.query(PlayerItems, Item, Warframe)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Warframe, Warframe.name == Item.name)
    companionQuery = db.session.query(PlayerItems, Item, Companion)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Companion, Companion.name == Item.name)

    united = weaponQuery.union(warframeQuery, companionQuery)\
        .filter(Item.name.ilike(f"%{filters["name"]}%"))

    if filters["type"] != "":
        united = united.filter(Weapon.item_class == filters["type"])

    print(filters)

    if filters["state"] != "all":
        if filters["state"] == "2":
            united = united.filter(PlayerItems.state == None)
        else:
            united = united.filter(PlayerItems.state == filters["state"])

    # if filters["mastered"] and not filters["unmastered"]:
    #    united = united.filter(PlayerItems.mastered == "t")
    # elif not filters["mastered"] and filters["unmastered"]:
    #    united = united.filter((PlayerItems.mastered == "f") | (PlayerItems.mastered == None))
    # elif not filters["mastered"] and not filters["unmastered"]:
    #    united = united.filter(False)

    items: list[tuple[PlayerItems, Item, Weapon]] = united\
        .order_by(sortOrder[0])\
        .order_by(sortOrder[1])\
        .order_by(sortOrder[2])\
        .all()

    items_send = []

    for item in items:
        state: int = 2

        if item[0]:
            state = item[0].state

        items_send.append({
            "name": item[1].name,
            "class": item[2].item_class,
            "state": state
        })

    if request.method == "GET":
        return render("progress/progress.html", itemList=items_send)
    else:
        return render_template("progress/progress_table_raw.html", itemList=items_send)


@login_required
@app.route("/progress/import", methods=["GET", "POST"])
def import_progress():
    form: UploadFileForm = UploadFileForm()

    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    if not form.validate_on_submit():
        return render("progress/import.html", form=form)

    file: FileStorage = form.file.data

    # if "file" not in request.files:
    #    flash("No file uploaded.", "error")
    #    return redirect(request.url)

    # file = request.files["file"]

    if not file:
        flash("No file uploaded.", "error")
        return redirect(request.url)

    if file.filename == "":
        flash("No file uploaded.", "error")
        return redirect(request.url)

    if not file.filename.endswith(".json"):
        flash("Invalid file format (expected .json).", "error")
        return redirect(request.url)

    filename = f"import_{time.time()}_{getattr(current_user, "username")}.json"
    file.save(os.path.join(app.config["UPLOAD_FOLDER"], filename))

    with open(os.path.join(app.config["UPLOAD_FOLDER"], filename), "r") as tempfile:
        try:
            json_raw = tempfile.read()
        except:
            tempfile.close()
            flash("Couldn't read file.", "error")
            return redirect("/progress/import")
        tempfile.close()
        os.remove(os.path.join(app.config["UPLOAD_FOLDER"], filename))

    try:
        parsed = json.loads(json_raw)
    except:
        flash("Invalid JSON file.", "error")
        return redirect("/progress/import")

    weaponQuery = db.session.query(PlayerItems, Item, Weapon)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Weapon, Weapon.name == Item.name)
    warframeQuery = db.session.query(PlayerItems, Item, Warframe)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Warframe, Warframe.name == Item.name)
    companionQuery = db.session.query(PlayerItems, Item, Companion)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))\
        .join(Companion, Companion.name == Item.name)

    all_items_query = weaponQuery.union(warframeQuery, companionQuery)

    try:
        for json_item in parsed["XPInfo"]:
            db_item: tuple[PlayerItems, Item, Weapon] = all_items_query.filter(Item.nameraw == json_item["ItemType"]).first()
            if not db_item:
                continue
            if db_item[2].item_class == "Necramech":
                xp_required = 1600000
            elif "Kuva" in db_item[1].name or "Tenet" in db_item[1].name or db_item == "Paracesis":
                xp_required = 800000
            elif db_item[2].item_class in [
                "Amp",
                "Archgun",
                "Archmelee",
                "Kitgun",
                "Melee",
                "Primary",
                "Secondary",
                "Sentinel Weapon",
                "Zaw"
            ]:
                xp_required = 450000
            elif db_item[2].item_class in [
                "Archwing",
                "Hound",
                "Kdrive",
                "Moa",
                "Pet",
                "Sentinel",
                "Warframe"
            ]:
                xp_required = 900000
            else:
                raise Exception("nejaka picovina mi utekla")

            if json_item["XP"] >= xp_required:
                state = 0
            else:
                state = 1

            if db_item[0]:
                db_item[0].state = state
            else:
                new_item = PlayerItems(player_id=player.id, item_name=db_item[1].name, state=state)
                db.session.add(new_item)
    except:
        flash("Invalid JSON file.", "error")
        return redirect("/progress/import")

    db.session.commit()

    flash("Imported successfully", "info")
    return redirect("/progress")


if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)
