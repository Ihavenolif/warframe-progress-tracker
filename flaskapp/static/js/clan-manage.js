/**
 * 
 * @param {string} url 
 * @param {object} data 
 */
function requrestWrapper(url, data) {
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            // Typical action to be performed when the document is ready:
            alert(xhttp.responseText)
            switch (url.split("/")[url.split("/").length - 1]) {
                case "delete":
                    window.location = window.location
                    break;
                case "promote":
                    window.location = "/clans"
                    break;
                case "leave":
                    window.location = "/clans"
                    break;
                default:
                    break;
            }
        }
    };
    xhttp.open("POST", url, true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify(data));
}

function remove(player, clan) {
    if (confirm("Do you really want to remove player" + player + " from clan " + clan + "?")) {
        requrestWrapper("/clans/" + clan + "/delete", { "name": player })
    }
}

function promote(player, clan) {
    if (confirm("Do you really want to promote player" + player + " to leader of clan " + clan + "?")) {
        requrestWrapper("/clans/" + clan + "/promote", { "name": player })
    }
}

function leave(clan) {
    if (confirm("Do you really want to leave clan " + clan + "?")) {
        requrestWrapper("/clans/" + clan + "/leave", {})
    }
}