import hashlib

from flask import flash, redirect
from flask_login import login_user, logout_user
from flask_wtf import FlaskForm
from wtforms import StringField, PasswordField
from wtforms.validators import DataRequired, EqualTo

from database import db
from models.registered_user import Registered_user
from flask_app import app
from util import render, generate_random_password


class RegisterForm(FlaskForm):
    username = StringField("Username", validators=[DataRequired()])
    password = PasswordField("Password", validators=[DataRequired(), EqualTo("password_repeat", message="Passwords must match")])
    password_repeat = PasswordField("Repeat password", validators=[DataRequired()])


@app.route("/register", methods=["GET", "POST"])
def register():
    form: RegisterForm = RegisterForm()

    if not form.validate_on_submit():
        try:
            error = form.errors["password"][0]
            flash(error, "error")
        except:
            pass
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        return render("register.html", form=form)

    username: str = form.username.data

    if Registered_user.query.filter_by(username=username).first():
        flash("That username is already taken.", "error")
        return render("register.html", form=form)

    password: str = form.password.data
    salt = generate_random_password(32)
    password_hash = hashlib.sha256((password+salt).encode("utf-8")).hexdigest()

    user: Registered_user = Registered_user(username=username, password_hash=password_hash, salt=salt)
    db.session.add(user)
    db.session.commit()
    logout_user()
    login_user(user)
    flash("Registered with username " + username + ".", "info")
    return redirect("/")
