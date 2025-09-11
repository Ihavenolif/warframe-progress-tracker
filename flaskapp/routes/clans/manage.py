from flask import redirect, flash, request
from flask_login import login_required, current_user

from database import db
from flask_app import app
from models.clan import Clan
from models.player import Player
from models.player_clan import PlayerClan
from util import render


@app.route("/clans/<name>/manage")
@login_required
def manage(name):
    clan: Clan = Clan.query.filter(Clan.name == name).first()
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not clan:
        flash("Clan not found.", "error")
        return redirect("/clans")

    if not player:
        flash("You need to link your warframe account first.", "error")
        return redirect("/settings")

    if not clan.leader_id == player.id:
        flash("You are not allowed to manage this clan.", "error")
        return redirect("/clans")

    clan_players = []

    for clan_player in db.session.query(Player).join(PlayerClan, (Player.id == PlayerClan.player_id) & (PlayerClan.clan_id == clan.id)).order_by(Player.username).all():
        clan_players.append({
            "name": clan_player.username,
            "rank": clan_player.mastery_rank,
            "is_leader": clan_player.id == clan.leader_id
        })

    return render("clans/manage.html", clan_members=clan_players, clan_name=clan.name)


@app.route("/clans/<name>/leave", methods=["POST"])
@login_required
def leave(name):
    clan: Clan = Clan.query.filter(Clan.name == name).first()
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not clan:
        return "Clan not found", 404

    if not player:
        return "You need to link your warframe account first", 403

    if player.id == clan.leader_id:
        return "You cannot leave a clan you are a leader of", 400

    player_clan: PlayerClan = PlayerClan.query.filter(PlayerClan.clan_id == clan.id).filter(PlayerClan.player_id == player.id).first()

    if not player_clan:
        return "You are not a member of this clan", 400

    db.session.delete(player_clan)
    db.session.commit()

    return "Clan successfully left", 200


@app.route("/clans/<name>/delete", methods=["POST"])
@login_required
def delete(name):
    if request.content_type == "application/json":
        try:
            target_player_name = request.json["name"]
        except:
            return "Bad JSON format", 400
    else:
        return "Bad request, application/json expected", 400

    clan: Clan = Clan.query.filter(Clan.name == name).first()
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()
    target_player: Player = Player.query.filter_by(username=target_player_name).first()

    if not clan:
        return "Clan not found", 404

    if not player:
        return "You need to link your warframe account first", 403

    if not player.id == clan.leader_id:
        return "You are not a leader of this clan", 400

    player_clan: PlayerClan = db.session.query(PlayerClan).filter(PlayerClan.player_id == target_player.id).first()

    if not player_clan:
        return "That player is not a part of your clan", 400

    db.session.delete(player_clan)
    db.session.commit()

    return f"Player {target_player_name} successfully deleted", 200


@app.route("/clans/<name>/promote", methods=["POST"])
@login_required
def promote(name):
    if request.content_type == "application/json":
        try:
            target_player_name = request.json["name"]
        except:
            return "Bad JSON format", 400
    else:
        return "Bad request, application/json expected", 400

    clan: Clan = Clan.query.filter(Clan.name == name).first()
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()
    target_player: Player = Player.query.filter_by(username=target_player_name).first()

    if not clan:
        return "Clan not found", 404

    if not player:
        return "You need to link your warframe account first", 403

    if not player.id == clan.leader_id:
        return "You are not a leader of this clan", 400

    player_clan: PlayerClan = db.session.query(PlayerClan).filter(PlayerClan.player_id == target_player.id).first()

    if not player_clan:
        return "That player is not a part of your clan", 400

    clan.leader_id = target_player.id
    db.session.commit()

    return f"Player {target_player_name} was promoted to leader successfully", 200
