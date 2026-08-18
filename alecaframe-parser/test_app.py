import importlib
import io
import json
import sys
import types
import unittest
import urllib.error
from unittest.mock import MagicMock, patch


def load_app():
    tkinter = types.ModuleType("tkinter")
    tkinter.Tk = MagicMock(return_value=MagicMock())
    tkinter.StringVar = MagicMock(side_effect=lambda *args, **kwargs: MagicMock())
    tkinter.BooleanVar = MagicMock(side_effect=lambda *args, **kwargs: MagicMock())
    tkinter.ttk = MagicMock()

    crypto = types.ModuleType("Crypto")
    cipher = types.ModuleType("Crypto.Cipher")
    cipher.AES = MagicMock()
    util = types.ModuleType("Crypto.Util")
    padding = types.ModuleType("Crypto.Util.Padding")
    padding.unpad = MagicMock()

    modules = {
        "tkinter": tkinter,
        "tkinter.ttk": tkinter.ttk,
        "Crypto": crypto,
        "Crypto.Cipher": cipher,
        "Crypto.Util": util,
        "Crypto.Util.Padding": padding,
    }
    with patch.dict(sys.modules, modules):
        sys.modules.pop("app", None)
        return importlib.import_module("app")


def response(payload=b""):
    result = MagicMock()
    result.__enter__.return_value = result
    result.read.return_value = payload
    return result


def http_error(code, body=b"invalid token"):
    return urllib.error.HTTPError(
        "https://example.test/api/mastery/update",
        code,
        "Unauthorized",
        {},
        io.BytesIO(body),
    )


class SendJsonTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.app = load_app()

    def setUp(self):
        self.app.auth_token = "expired-token"
        self.app.base_url_var.get.return_value = "https://example.test"
        self.app.username_var.get.return_value = "tester"
        self.app.password_var.get.return_value = "password"
        self.app.set_status = MagicMock()

    def test_successful_post_does_not_login(self):
        with patch.object(self.app, "login") as login_mock, patch.object(
            self.app.urllib.request,
            "urlopen",
            return_value=response(),
        ) as urlopen_mock:
            result = self.app.send_json("{}")

        self.assertTrue(result)
        login_mock.assert_not_called()
        self.assertEqual(1, urlopen_mock.call_count)

    def test_auth_failure_logs_in_and_retries_post_once(self):
        login_payload = json.dumps({"token": "renewed-token", "username": "tester"}).encode()
        with patch.object(
            self.app.urllib.request,
            "urlopen",
            side_effect=[http_error(401), response(login_payload), response()],
        ) as urlopen_mock:
            result = self.app.send_json("{}")

        self.assertTrue(result)
        self.assertEqual(3, urlopen_mock.call_count)
        first_post = urlopen_mock.call_args_list[0].args[0]
        retried_post = urlopen_mock.call_args_list[2].args[0]
        self.assertEqual("Bearer expired-token", first_post.get_header("Authorization"))
        self.assertEqual("Bearer renewed-token", retried_post.get_header("Authorization"))

    def test_login_failure_stops_post_retry(self):
        with patch.object(
            self.app.urllib.request,
            "urlopen",
            side_effect=[http_error(401), http_error(401, b"bad credentials")],
        ) as urlopen_mock:
            result = self.app.send_json("{}")

        self.assertFalse(result)
        self.assertEqual(2, urlopen_mock.call_count)
        self.assertIsNone(self.app.auth_token)
        self.assertTrue(self.app.set_status.call_args.args[0].startswith("Login failed:"))

    def test_failed_retry_does_not_start_another_login(self):
        login_payload = json.dumps({"token": "renewed-token", "username": "tester"}).encode()
        with patch.object(
            self.app.urllib.request,
            "urlopen",
            side_effect=[http_error(403), response(login_payload), http_error(401)],
        ) as urlopen_mock:
            result = self.app.send_json("{}")

        self.assertFalse(result)
        self.assertEqual(3, urlopen_mock.call_count)
        self.app.set_status.assert_called_with(
            "Send failed: server returned 401: invalid token"
        )


if __name__ == "__main__":
    unittest.main()
