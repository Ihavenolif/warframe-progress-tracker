from flask import request, flash, redirect, render_template
from flask_login import login_required, current_user
from sqlalchemy import desc
from sqlalchemy.orm import aliased

from database import db
from flask_app import app
from models.clan import Clan
from models.item import Item
from models.player import Player
from models.player_clan import PlayerClan
from models.player_items import PlayerItems
from util import render


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
        return redirect("/clans")

    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    if not PlayerClan.query.filter(PlayerClan.clan_id == clan.id).filter(PlayerClan.player_id == player.id).first():
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
