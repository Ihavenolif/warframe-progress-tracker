from database import db


class PlayerItems(db.Model):
    player_id = db.Column(db.Integer, primary_key=True)
    item_name = db.Column(db.String(256), primary_key=True)
    state = db.Column(db.Integer)
