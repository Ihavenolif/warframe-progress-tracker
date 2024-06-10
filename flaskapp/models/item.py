from database import db


class Item(db.Model):
    name = db.Column(db.String(256), primary_key=True)
    nameraw = db.Column(db.String(256))
    type = db.Column(db.String(256))
    item_class = db.Column(db.String(256))
