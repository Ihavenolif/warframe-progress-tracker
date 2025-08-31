//export const BASE_URL = "http://localhost:5224"
export const BASE_URL = "https://www.localhost.me:8080"

export async function getPlayerName() {
    const token = localStorage.getItem("token");

    if (!token) window.location.href = "login";

    console.log("kokot1")

    const res = await fetch(`${BASE_URL}/api/auth/me`, {
        headers: {
            'Authorization': `Bearer ${token}`
        },
        method: "POST"
    });

    console.log("kokot2")

    if (res.status == 401) {
        localStorage.removeItem("token");
        localStorage.removeItem("username");
        window.location.href = "login";
    }

    const data = await res.json();
    console.table(data)
    return data["playerName"]
}