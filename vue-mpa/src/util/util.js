//export const BASE_URL = "http://localhost:5224"
import { emit, TokenUpdateSignal } from "./signals";
import { store } from "@/store";
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
        // localStorage.setItem("token", data.token);
        store.commit('setCredentials', {
            token: data.token,
            username: data.username
        });
        emit(TokenUpdateSignal);
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

export function parseJwt(token) {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
}

export function getRoles(token) {
    return token["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || [];
}

const warframeThresholds = [
    0, 1000, 4000, 9000, 16000, 25000, 36000, 49000, 64000, 81000,
    100000, 121000, 144000, 169000, 196000, 225000, 256000, 289000,
    324000, 361000, 400000, 441000, 484000, 529000, 576000, 625000,
    676000, 729000, 784000, 841000, 900000, 961000, 1024000, 1089000,
    1156000, 1225000, 1296000, 1369000, 1444000, 1521000, 1600000
];

const weaponThresholds = [
    0, 500, 2000, 4500, 8000, 12500, 18000, 24500, 32000, 40500,
    50000, 60500, 72000, 84500, 98000, 112500, 128000, 144500,
    162000, 180500, 200000, 220500, 242000, 264500, 288000, 312500,
    338000, 364500, 392000, 420500, 450000, 480500, 512000, 544500,
    578000, 612500, 648000, 684500, 722000, 760500, 800000
];

export function getMaxRank(xp_required) {
    return xp_required == 1600000 || xp_required == 800000 ? 40 :
        xp_required == 900000 || xp_required == 450000 ? 30 :
            (console.error("Unknown max rank for item", xp_required), 0);
}

export function getRank(xp_required, xp_gained) {
    const thresholds = xp_required == 1600000 || xp_required == 900000 ? warframeThresholds :
        xp_required == 800000 || xp_required == 450000 ? weaponThresholds :
            (console.error("Unknown thresholds for item", xp_required), []);

    if (thresholds.length == 0) return -1;

    const maxLevel = getMaxRank(xp_required);

    for (let level = 1; level < maxLevel + 1; level++) {
        if (xp_gained < thresholds[level])
            return level - 1;
    }

    // Max rank reached
    return maxLevel;
}