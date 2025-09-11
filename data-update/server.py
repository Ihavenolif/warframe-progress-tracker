from flask import Flask

app = Flask(__name__)


@app.route("/")
def index():
    return "Hello, World!"


@app.route("/health")
def health_check():
    return "OK", 200


@app.route("/data-update", methods=["POST"])
def data_update():
    from pg_send_data import send_data
    send_data()
    return "Data update initiated", 202


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
