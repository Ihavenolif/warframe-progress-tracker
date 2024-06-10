from flask import redirect, flash
from flask_login import login_required, current_user
from flask_wtf import FlaskForm
from wtforms import StringField, PasswordField
from wtforms.validators import DataRequired, EqualTo

from database import db
from flask_app import app
from models.player import Player
from util import render


class LinkAccountForm(FlaskForm):
    warframe_name = StringField("Account name", validators=[DataRequired()])


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
