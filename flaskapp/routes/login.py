import hashlib

from flask import flash, redirect
from flask_login import login_user
from flask_wtf import FlaskForm
from wtforms import StringField, PasswordField
from wtforms.validators import DataRequired

from models.registered_user import Registered_user
from flask_app import app
from util import render


class LoginForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired()])


@app.route("/login", methods=["GET", "POST"])
def login():
    form: LoginForm = LoginForm()
    if not form.validate_on_submit():
        return render("login.html", form=form)

    username = form.username.data
    password = form.password.data

    user: Registered_user = Registered_user.query.filter_by(username=username).first()

    if not user:
        # user not found
        flash("No user with that username exists.", "error")
        return render("login.html", form=form)

    password_hash = hashlib.sha256((password+user.salt).encode("utf-8")).hexdigest()
    if user.password_hash == password_hash:
        login_user(user)
    else:
        # incorrect password
        flash("Incorrect password.", "error")
        return render("login.html", form=form)
    flash("Successfully logged in.", "info")
    return redirect("/")
