from flask_app import app
from util import render


@app.route("/")
def index():
    return render("index.html")
