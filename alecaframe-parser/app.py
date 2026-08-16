import os
import json
import sys
import mimetypes
import time
import urllib.error
import urllib.request
import uuid

import tkinter as tk
from tkinter import ttk

from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad

AES_KEY = bytes([76, 69, 79, 45, 65, 76, 69, 67, 9, 69, 79, 45, 65, 76, 69, 67])
AES_IV = bytes([49, 50, 70, 71, 66, 51, 54, 45, 76, 69, 51, 45, 113, 61, 57, 0])
DEFAULT_BASE_URL = "https://warframe.mwll.cz"
WATCH_INTERVAL_MS = 2000

root = tk.Tk()
root.title("Alecaframe parser")
root.geometry("620x360")

auth_token = None
last_processed_mtime = None

def resource_path(relative_path):
    """ Get absolute path to resource, works for dev and for PyInstaller """
    base_path = getattr(sys, "_MEIPASS", os.path.abspath("."))

    return os.path.join(base_path, relative_path)


def app_dir():
    if getattr(sys, "frozen", False):
        return os.path.dirname(sys.executable)
    return os.path.dirname(os.path.abspath(__file__))

root.iconbitmap(resource_path("transparent.ico"))

main_frame = ttk.Frame(root, padding=12)
main_frame.pack(fill="both", expand=True)

ttk.Label(main_frame, text="Export AlecaFrame data to out.json, or send it directly to the server.").grid(row=0, column=0, columnspan=3, sticky="w", pady=(0, 12))

ttk.Label(main_frame, text="Base URL").grid(row=1, column=0, sticky="w", pady=4)
base_url_var = tk.StringVar(value=DEFAULT_BASE_URL)
ttk.Entry(main_frame, textvariable=base_url_var, width=45).grid(row=1, column=1, columnspan=2, sticky="ew", pady=4)

ttk.Label(main_frame, text="Username").grid(row=2, column=0, sticky="w", pady=4)
username_var = tk.StringVar()
ttk.Entry(main_frame, textvariable=username_var, width=30).grid(row=2, column=1, sticky="ew", pady=4)

ttk.Label(main_frame, text="Password").grid(row=3, column=0, sticky="w", pady=4)
password_var = tk.StringVar()
ttk.Entry(main_frame, textvariable=password_var, width=30, show="*").grid(row=3, column=1, sticky="ew", pady=4)

watch_var = tk.BooleanVar(value=False)
auto_send_var = tk.BooleanVar(value=False)
ttk.Checkbutton(main_frame, text="Watch AlecaFrame file", variable=watch_var, command=lambda: update_watch_status()).grid(row=4, column=0, columnspan=2, sticky="w", pady=(12, 4))
ttk.Checkbutton(main_frame, text="Automatically send after export", variable=auto_send_var).grid(row=5, column=0, columnspan=2, sticky="w", pady=4)

button_frame = ttk.Frame(main_frame)
button_frame.grid(row=6, column=0, columnspan=3, sticky="w", pady=(16, 8))
ttk.Button(button_frame, text="Login", command=lambda: login()).pack(side="left", padx=(0, 8))
ttk.Button(button_frame, text="Export data", command=lambda: parse_file(send_after_export=False)).pack(side="left", padx=(0, 8))
ttk.Button(button_frame, text="Send now", command=lambda: send_current_file()).pack(side="left")

res_label = ttk.Label(main_frame, text="Not logged in.", wraplength=560)
res_label.grid(row=7, column=0, columnspan=3, sticky="w", pady=(12, 0))

main_frame.columnconfigure(1, weight=1)


def alecaframe_data_path():
    return os.path.join(os.path.expanduser("~"), "AppData", "Local", "AlecaFrame", "lastData.dat")


def output_json_path():
    return os.path.join(app_dir(), "out.json")


def set_status(text):
    res_label.config(text=text)


def normalize_base_url():
    base_url = base_url_var.get().strip().rstrip("/")
    if not base_url:
        raise ValueError("Base URL is required.")
    if not base_url.startswith(("http://", "https://")):
        base_url = "https://" + base_url
    return base_url


def request_json(url, method="GET", headers=None, data=None, timeout=30):
    request = urllib.request.Request(url, data=data, headers=headers or {}, method=method)
    try:
        with urllib.request.urlopen(request, timeout=timeout) as response:
            raw = response.read().decode("utf-8")
            return json.loads(raw) if raw else None
    except urllib.error.HTTPError as error:
        message = error.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"Server returned {error.code}: {message or error.reason}") from error
    except urllib.error.URLError as error:
        raise RuntimeError(f"Connection failed: {error.reason}") from error


def login():
    global auth_token

    username = username_var.get().strip()
    password = password_var.get()
    if not username or not password:
        set_status("Username and password are required.")
        return

    try:
        base_url = normalize_base_url()
        credentials = json.dumps({"username": username, "password": password}).encode("utf-8")
        response = request_json(
            f"{base_url}/api/auth/login",
            method="POST",
            headers={"Content-Type": "application/json"},
            data=credentials,
        ) or {}
        token = response.get("token") if response else None
        if not token:
            raise RuntimeError("Login response did not contain an auth token.")
        auth_token = token
        set_status(f"Logged in as {response.get('username', username)}.")
    except Exception as error:
        auth_token = None
        set_status(f"Login failed: {error}")


def parse_file(send_after_export=None):
    path = alecaframe_data_path()
    if not os.path.exists(path):
        set_status(f"AlecaFrame data file not found: {path}")
        return None

    with open(path, "rb") as binfile:
        contents = binfile.read()

    try:
        res = decrypt_aes(contents, AES_KEY, AES_IV)
    except Exception as error:
        set_status(f"Couldn't decrypt AlecaFrame data: {error}")
        return None

    try:
        parsed = json.loads(res)
    except json.JSONDecodeError:
        set_status("JSON parse error.")
        return None

    try:
        parsed["XPInfo"]
        res_json = res
    except KeyError:
        try:
            res_json = parsed["InventoryJson"]
        except KeyError:
            set_status("Invalid JSON file.")
            return None

    if not isinstance(res_json, str):
        res_json = json.dumps(res_json)

    if not res_json.strip():
        set_status("Export failed: parsed JSON content is empty.")
        return None

    out_path = output_json_path()
    temp_path = out_path + ".tmp"
    try:
        with open(temp_path, "w", encoding="utf-8") as resfile:
            bytes_written = resfile.write(res_json)
        os.replace(temp_path, out_path)
    except OSError as error:
        set_status(f"Couldn't write out.json: {error}")
        return None

    set_status(f"Data successfully exported ({bytes_written} chars) to {out_path}.")

    if send_after_export is None:
        send_after_export = auto_send_var.get()
    if send_after_export:
        send_json(res_json)

    return res_json


def build_multipart_form_data(field_name, filename, content):
    boundary = f"----WarframeTracker{uuid.uuid4().hex}"
    content_type = mimetypes.guess_type(filename)[0] or "application/json"
    body = (
        f"--{boundary}\r\n"
        f"Content-Disposition: form-data; name=\"{field_name}\"; filename=\"{filename}\"\r\n"
        f"Content-Type: {content_type}\r\n\r\n"
    ).encode("utf-8")
    body += content.encode("utf-8")
    body += f"\r\n--{boundary}--\r\n".encode("utf-8")
    return body, f"multipart/form-data; boundary={boundary}"


def ensure_logged_in():
    if not auth_token:
        login()
    return auth_token


def send_json(json_content):
    token = ensure_logged_in()
    if not token:
        return False

    try:
        base_url = normalize_base_url()
        body, content_type = build_multipart_form_data("jsonFile", "out.json", json_content)
        request = urllib.request.Request(
            f"{base_url}/api/mastery/update",
            data=body,
            headers={
                "Authorization": f"Bearer {token}",
                "Content-Type": content_type,
                "Content-Length": str(len(body)),
            },
            method="POST",
        )
        with urllib.request.urlopen(request, timeout=60) as response:
            response.read()
        set_status(f"Data sent successfully at {time.strftime('%H:%M:%S')}.")
        return True
    except urllib.error.HTTPError as error:
        message = error.read().decode("utf-8", errors="replace")
        set_status(f"Send failed: server returned {error.code}: {message or error.reason}")
    except urllib.error.URLError as error:
        set_status(f"Send failed: connection failed: {error.reason}")
    except Exception as error:
        set_status(f"Send failed: {error}")
    return False


def send_current_file():
    parse_file(send_after_export=True)


def update_watch_status():
    global last_processed_mtime

    if watch_var.get():
        path = alecaframe_data_path()
        last_processed_mtime = os.path.getmtime(path) if os.path.exists(path) else None
        set_status("Watching AlecaFrame data file for changes.")
    else:
        set_status("File watch disabled.")


def watch_file():
    global last_processed_mtime

    if watch_var.get():
        path = alecaframe_data_path()
        if os.path.exists(path):
            mtime = os.path.getmtime(path)
            if last_processed_mtime is None or mtime > last_processed_mtime:
                last_processed_mtime = mtime
                parse_file(send_after_export=auto_send_var.get())
    root.after(WATCH_INTERVAL_MS, watch_file)


def decrypt_aes(ciphertext, key, iv):
    cipher = AES.new(key, AES.MODE_CBC, iv)
    decrypted_text = cipher.decrypt(ciphertext)
    plaintext = unpad(decrypted_text, AES.block_size)
    return plaintext.decode('utf-8')




def main() -> int:
    watch_file()
    root.mainloop()
    return 0

if __name__ == "__main__":
    main()
    #exit(main())
