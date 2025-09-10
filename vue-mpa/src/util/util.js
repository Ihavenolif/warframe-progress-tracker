//export const BASE_URL = "http://localhost:5224"
export const BASE_URL = window.location.origin

export async function getPlayerName() {
    const res = await authFetch("/api/auth/me", {
        method: "POST"
    });

    if (res.status == 401) {
        localStorage.removeItem("token");
        localStorage.removeItem("username");
        window.location.href = "login";
    }

    const data = await res.json();
    console.table(data)
    return data["playerName"]
}

let refreshingTokenPromise = null;

async function refreshToken() {
    if (refreshingTokenPromise) return refreshingTokenPromise;

    refreshingTokenPromise = (async () => {
        const response = await fetch(`${BASE_URL}/api/auth/refresh`, {
            method: "POST",
            credentials: 'include' // send cookies
        });

        if (!response.ok) {
            refreshingTokenPromise = null;
            return null;
        }

        const data = await response.json();
        localStorage.setItem("token", data.token);
        refreshingTokenPromise = null;
        return data.token;
    })();

    return refreshingTokenPromise;
}

export async function authFetch(url, options = {}) {
    if (!options.headers) options.headers = {};
    const token = localStorage.getItem("token");

    if (token) options.headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(`${BASE_URL}${url}`, options);

    if (res.status != 401) {
        return res;
    }

    const newToken = await refreshToken();

    if (!newToken) {
        localStorage.removeItem("token");
        localStorage.removeItem("username");

        window.location.href = "/login";
        return;
    }

    options.headers['Authorization'] = `Bearer ${newToken}`;
    return fetch(`${BASE_URL}${url}`, options);
}