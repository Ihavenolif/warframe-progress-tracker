from database import db


class Clan(db.Model):
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    name = db.Column(db.String(256))
    leader_id = db.Column(db.Integer)
