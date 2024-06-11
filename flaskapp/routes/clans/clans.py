from flask import redirect, flash
from flask_login import login_required, current_user

from database import db
from flask_app import app
from models.clan import Clan
from models.player import Player
from models.player_clan import PlayerClan
from util import render


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
        leader = db.session.query(Player).filter(Player.id == clan.leader_id).first().username
        clan_list.append({
            "name": clan.name,
            "members": PlayerClan.query.filter(clan.id == PlayerClan.clan_id).all().__len__(),
            "leader": leader,
            "is_leader": leader == player.username
        })
    return render("clans/clans.html", clan_list=clan_list)
