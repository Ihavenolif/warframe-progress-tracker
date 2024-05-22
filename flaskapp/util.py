from flask import render_template
import random
import string

characters = list(string.ascii_letters + string.digits + "!@#$%^&*()")


def generate_random_password(length):
    # shuffling the characters
    random.shuffle(characters)

    # picking random characters from the list
    password = []
    for i in range(length):
        password.append(random.choice(characters))

    # shuffling the resultant password
    random.shuffle(password)

    # converting the list to string
    # printing the list
    return "".join(password)


def render(url: str, *args, **kwargs):
    return render_template("shared/base.html", content=render_template(url, **kwargs), **kwargs)
