import hashlib

from flask import redirect, flash
from flask_login import login_required, current_user
from flask_wtf import FlaskForm
from wtforms import StringField, PasswordField
from wtforms.validators import DataRequired, EqualTo

from database import db
from flask_app import app
from util import render


class ChangePasswordForm(FlaskForm):
    old_password = PasswordField("Old password", validators=[DataRequired()])
    new_password = PasswordField("New password", validators=[DataRequired(), EqualTo("password_repeat", message="Passwords must match")])
    password_repeat = PasswordField("Repeat password", validators=[DataRequired()])


@app.route("/change_password", methods=["GET", "POST"])
@login_required
def change_password():
    form: ChangePasswordForm = ChangePasswordForm()

    if not form.validate_on_submit():
        # return render_template("shared/base.html", form=form, content=render_template("register.html", form=form))
        try:
            error = form.errors["password"][0]
            flash(error, "error")
        except:
            pass
        return render("change_password.html", form=form)

    old_password: str = form.old_password.data
    salt = getattr(current_user, "salt")
    old_password_hash = hashlib.sha256((old_password+salt).encode("utf-8")).hexdigest()

    if old_password_hash != getattr(current_user, "password_hash"):
        flash("Old password is wrong.", "error")
        return render("change_password.html", form=form)

    new_password: str = form.new_password.data
    new_password_hash = hashlib.sha256((new_password+salt).encode("utf-8")).hexdigest()

    current_user.password_hash = new_password_hash
    db.session.commit()
    flash("Password changed successfully.", "info")
    return redirect("/settings")
