from database import db


class PlayerClan(db.Model):
    player_id = db.Column(db.Integer, primary_key=True)
    clan_id = db.Column(db.Integer, primary_key=True)
