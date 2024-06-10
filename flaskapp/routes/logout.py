from flask import flash, redirect
from flask_login import login_required, logout_user

from flask_app import app


@app.route("/logout")
@login_required
def logout():
    logout_user()
    flash("Successfully logged out.", "info")
    return redirect("/")
