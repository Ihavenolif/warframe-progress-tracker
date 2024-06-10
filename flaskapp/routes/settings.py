from flask_login import login_required, current_user

from flask_app import app
from models.player import Player
from util import render


@app.route("/settings")
@login_required
def settings():
    player = Player.query.filter_by(registered_user_id=getattr(current_user, "id")).first()
    if player:
        warframe_name = player.username
    else:
        warframe_name = ""
    return render("settings.html", warframe_name=warframe_name)
