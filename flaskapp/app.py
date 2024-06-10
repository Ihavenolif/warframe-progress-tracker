from wtforms.validators import DataRequired, EqualTo
from wtforms import SubmitField, StringField, PasswordField
from flask_wtf import FlaskForm
from flask_wtf.file import FileField, FileRequired, FileStorage
from flask import Flask, redirect, flash, request, render_template, send_file
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
    item_class = db.Column(db.String(256))


class PlayerItems(db.Model):
    player_id = db.Column(db.Integer, primary_key=True)
    item_name = db.Column(db.String(256), primary_key=True)
    state = db.Column(db.Integer)


class Clan(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(256))
    leader_id = db.Column(db.Integer)


class PlayerClan(db.Model):
    player_id = db.Column(db.Integer, primary_key=True)
    clan_id = db.Column(db.Integer, primary_key=True)


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


@app.route("/logout")
@login_required
def logout():
    logout_user()
    flash("Successfully logged out.", "info")
    return redirect("/")


@app.route("/clans")
@login_required
def clans():
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    player_clans_db: list[PlayerClan] = PlayerClan.query.filter(PlayerClan.player_id == player.id).all()
    player_clans: list[Clan] = []

    for db_item in player_clans_db:
        player_clans.append(Clan.query.filter(Clan.id == db_item.clan_id).first())

    clan_list = []
    for clan in player_clans:
        clan_list.append({
            "name": clan.name,
            "members": PlayerClan.query.filter(clan.id == PlayerClan.clan_id).all().__len__()
        })
    return render("clans/clans.html", clan_list=clan_list)


@app.route("/clans/<name>/progress", methods=["GET", "POST"])
@login_required
def clan_progress(name):

    if request.content_type == "application/json":
        sort_type_dictionary = {
            "state": PlayerItems.state,
            "name": Item.name,
            "class": Item.item_class
        }
        sortJson = request.json["sorting"]
        sortOrder: list = []

        for sort in sortJson:
            if sort["type"] in ["name", "class"]:
                temp = sort_type_dictionary[sort["type"]]
            else:
                temp = sort["type"].split("-")[0]

            if sort["ascending"]:
                sortOrder.append(temp)
            else:
                sortOrder.append(desc(temp))

        filters = request.json["filters"]

        print(request.json)
    else:
        sortOrder: list = [
            Item.item_class,
            Item.name
        ]

        filters = {
            "class": "",
            "name": ""
        }

    clan: Clan = Clan.query.filter(Clan.name == name).first()
    if not clan:
        flash("Clan not found.", "error")
        return redirect("/")

    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    if not PlayerClan.query.filter(PlayerClan.clan_id == name).filter(PlayerClan.player_id == player.id):
        flash("You are not a member of this clan.", "error")
        return redirect("/")

    newquery = db.session.query(Item)\
        .filter(Item.name.ilike(f"%{filters["name"]}%"))

    aliases: dict[str, PlayerItems] = {}
    clan_players = db.session.query(Player)\
        .join(PlayerClan, (PlayerClan.player_id == Player.id) & (PlayerClan.clan_id == clan.id))\
        .all()

    player_name_list = []

    for clan_player in clan_players:
        player_name_list.append(clan_player.username)
        alias = aliased(PlayerItems)
        aliases[clan_player.username] = alias
        newquery = newquery.outerjoin(alias, (Item.name == alias.item_name) & (alias.player_id == clan_player.id))
        newquery = newquery.add_columns(alias.state.label(clan_player.username))

    # newquery = newquery\
    #    .outerjoin(Weapon, Item.name == Weapon.name)\
    #    .outerjoin(Warframe, Item.name == Warframe.name)\
    #    .outerjoin(Companion, Item.name == Companion.name)\
    #    .add_columns(class_column)

    if filters["class"] != "":
        newquery = newquery.filter(Item.item_class == filters["class"])

    for sort in sortOrder:
        newquery = newquery.order_by(sort)

    all_items = newquery.all()

    send_item_list = []

    for item in all_items:
        players = []

        for i in range(1, clan_players.__len__() + 1):
            if item[i] == 0:
                players.append(0)
            elif item[i] == 1:
                players.append(1)
            else:
                players.append(2)

        send_item_list.append({
            "name": item[0].name,
            "class": item[0].item_class,
            "players": players
        })

    if request.method == "POST":
        return render_template("clans/clan_progress_body.html", itemList=send_item_list, playernames=player_name_list)

    return render("clans/clan_progress.html", itemList=send_item_list, playernames=player_name_list)


@app.route("/settings")
@login_required
def settings():
    player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()
    if player:
        warframe_name = player.username
    else:
        warframe_name = ""
    return render("settings.html", warframe_name=warframe_name)
    pass


@app.route("/change_password", methods=["GET", "POST"])
@login_required
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


@app.route("/link_account", methods=["GET", "POST"])
@login_required
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


@app.route("/unlink_account")
@login_required
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


@app.route("/progress", methods=["GET", "POST"])
@login_required
def progress():
    if request.content_type == "application/json":
        sort_type_dictionary = {
            "state": PlayerItems.state,
            "name": Item.name,
            "class": Item.item_class
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
            Item.item_class,
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

    query = db.session.query(Item, PlayerItems)\
        .filter(Item.name.ilike(f"%{filters["name"]}%"))

    if filters["type"] != "":
        query = query.filter(Item.item_class == filters["type"])

    query = query\
        .outerjoin(PlayerItems, (Item.name == PlayerItems.item_name) & (PlayerItems.player_id == player.id))

    if filters["state"] != "all":
        if filters["state"] == "2":
            query = query.filter(PlayerItems.state == None)
        else:
            query = query.filter(PlayerItems.state == filters["state"])

    # if filters["mastered"] and not filters["unmastered"]:
    #    united = united.filter(PlayerItems.mastered == "t")
    # elif not filters["mastered"] and filters["unmastered"]:
    #    united = united.filter((PlayerItems.mastered == "f") | (PlayerItems.mastered == None))
    # elif not filters["mastered"] and not filters["unmastered"]:
    #    united = united.filter(False)

    items: list[tuple[PlayerItems, Item]] = query\
        .order_by(sortOrder[0])\
        .order_by(sortOrder[1])\
        .order_by(sortOrder[2])\
        .all()

    items_send = []

    for item in items:
        state: int = 2

        if item[1]:
            state = item[1].state

        items_send.append({
            "name": item[0].name,
            "class": item[0].item_class,
            "state": state
        })

    if request.method == "GET":
        return render("progress/progress.html", itemList=items_send)
    else:
        return render_template("progress/progress_table_raw.html", itemList=items_send)


@app.route("/progress/import", methods=["GET", "POST"])
@login_required
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

    query = db.session.query(PlayerItems, Item)\
        .outerjoin(PlayerItems, (PlayerItems.item_name == Item.name) & (PlayerItems.player_id == player.id))

    try:
        for json_item in parsed["XPInfo"]:
            db_item: tuple[PlayerItems, Item] = query.filter(Item.nameraw == json_item["ItemType"]).first()
            if not db_item:
                continue
            if db_item[1].item_class == "Necramech":
                xp_required = 1600000
            elif "Kuva" in db_item[1].name or "Tenet" in db_item[1].name or db_item == "Paracesis":
                xp_required = 800000
            elif db_item[1].item_class in [
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
            elif db_item[1].item_class in [
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
