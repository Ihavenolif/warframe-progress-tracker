import json
import time
import os

from flask import redirect, flash, request
from flask_login import login_required, current_user
from flask_wtf import FlaskForm
from flask_wtf.file import FileField, FileStorage
from wtforms import SubmitField

from database import db
from flask_app import app
from models.item import Item
from models.player import Player
from models.player_items import PlayerItems
from util import render


class UploadFileForm(FlaskForm):
    file = FileField("File")
    submit = SubmitField("Upload File")


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
