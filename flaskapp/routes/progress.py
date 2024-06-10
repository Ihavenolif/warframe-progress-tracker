from flask import request, flash, redirect, render_template
from flask_login import login_required, current_user
from sqlalchemy import desc

from database import db
from flask_app import app
from models.item import Item
from models.player import Player
from models.player_items import PlayerItems
from util import render


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
