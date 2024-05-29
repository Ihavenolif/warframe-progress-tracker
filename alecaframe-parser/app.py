import os
import json
import sys

import tkinter as tk

from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad

AES_KEY = bytes([76, 69, 79, 45, 65, 76, 69, 67, 9, 69, 79, 45, 65, 76, 69, 67])
AES_IV = bytes([49, 50, 70, 71, 66, 51, 54, 45, 76, 69, 51, 45, 113, 61, 57, 0])

root = tk.Tk()
root.title("Alecaframe parser")
root.geometry("500x150")

def resource_path(relative_path):
    """ Get absolute path to resource, works for dev and for PyInstaller """
    try:
        # PyInstaller creates a temp folder and stores the path in _MEIPASS
        base_path = sys._MEIPASS
    except AttributeError:
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)

root.iconbitmap(resource_path("transparent.ico"))

# Create a label
label = tk.Label(root, text="Click the button below to export your data. Then, upload the out.json file to the website.")
label.pack(pady=10)

res_label = tk.Label(root, text="")
res_label.pack(pady=10)

def parse_file():
    binfile = open("C:\\Users\\" + os.environ.get("USERNAME") + "\\AppData\\Local\\AlecaFrame\\lastData.dat", "rb")
    contents = binfile.read()

    res = decrypt_aes(contents, AES_KEY, AES_IV)

    try:
        parsed = json.loads(res)
    except:
        res_label.config("JSON parse error.")
        return

    try:
        parsed["XPInfo"]
        res_json = res
    except:

        try:
            res_json = parsed["InventoryJson"]
        except:
            res_label.config("Invalid JSON file")
            return
        
    #return(res_json)

    with open("out.json", "w") as resfile:
        resfile.write(res_json)
        resfile.close()

    res_label.config(text="Data successfully exported")

# Create a button
button = tk.Button(root, text="Export data", command=parse_file)
button.pack(pady=10)

def decrypt_aes(ciphertext, key, iv):
    # Create an AES cipher object with the given key and IV
    cipher = AES.new(key, AES.MODE_CBC, iv)
    
    # Decrypt the ciphertext
    decrypted_text = cipher.decrypt(ciphertext)
    
    # Unpad the decrypted text to get the original plaintext
    plaintext = unpad(decrypted_text, AES.block_size)
    
    return plaintext.decode('utf-8')




def main() -> int:
    # Run the application
    root.mainloop()

if __name__ == "__main__":
    main()
    #exit(main())