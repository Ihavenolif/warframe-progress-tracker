from database import db


class Player(db.Model):
    username = db.Column(db.String(256))
    id = db.Column(db.Integer, primary_key=True)
    registered_user_id = db.Column(db.Integer)
    mastery_rank = db.Column(db.Integer)
