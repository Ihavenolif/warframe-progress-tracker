from flask import flash, redirect
from flask_login import login_required, current_user
from flask_wtf import FlaskForm
from wtforms import StringField, PasswordField
from wtforms.validators import DataRequired, EqualTo

from database import db
from flask_app import app
from models.clan import Clan
from models.player import Player
from models.player_clan import PlayerClan
from util import render


class CreateClanForm(FlaskForm):
    clan_name = StringField("Clan name", validators=[DataRequired()])


@app.route("/clans/create", methods=["POST", "GET"])
@login_required
def create_clan():
    form: CreateClanForm = CreateClanForm()
    player: Player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()

    if not form.validate_on_submit():
        try:
            error = form.errors["name"][0]
            flash(error, "error")
        except:
            pass
        return render("clans/create_clan.html", form=form)

    clan_name: str = form.clan_name.data

    if Clan.query.filter_by(name=clan_name).first():
        flash("A Clan with that name already exists.", "error")
        return render("clans/create_clan.html", form=form)

    clan = Clan(name=clan_name, leader_id=player.id)
    db.session.add(clan)
    db.session.commit()
    player_clan = PlayerClan(player_id=player.id, clan_id=clan.id)
    db.session.add(player_clan)
    db.session.commit()

    flash("Clan " + clan_name + " successfully created.", "info")
    return redirect("/clans")
