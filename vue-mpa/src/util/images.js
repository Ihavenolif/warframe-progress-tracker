import localforage from "localforage";
import { authFetch, BASE_URL } from "./util";
import { ManifestFetchStartedSignal, ManifestFetchFinishedSignal, ManifestLoadStartedSignal, ManifestLoadFinishedSignal, ManifestParseStartedSignal, ManifestParseFinishedSignal, emit } from "./signals";

localforage.config({
    name: 'warframe-tracker',
    storeName: 'images'
})

let updateImageDbPromise = null;

async function getIndex() {
    emit(ManifestFetchStartedSignal);
    const res = await authFetch("/api/items/index");
    emit(ManifestFetchFinishedSignal);
    return await res.json();
}

async function updateImageDb() {
    if (updateImageDbPromise) {
        return updateImageDbPromise;
    }
    updateImageDbPromise = (async () => {
        const index = await getIndex();

        emit(ManifestLoadStartedSignal);
        const res = await fetch("https://content.warframe.com/PublicExport/Manifest/" + index["Manifest"]);
        const imageLinks = (await res.json())["Manifest"]
        emit(ManifestLoadFinishedSignal);

        emit(ManifestParseStartedSignal);
        imageLinks.forEach(item => {
            localforage.setItem(item["uniqueName"], item["textureLocation"]);
        });
        emit(ManifestParseFinishedSignal);

    })();

    return updateImageDbPromise;
}

export async function getImage(uniqueName) {
    if (uniqueName == "/Lotus/Types/Game/CrewShip/RailJack/DefaultHarness") {
        return "https://wiki.warframe.com/images/Plexus.png?25d71";
    }
    const image = await localforage.getItem(uniqueName);
    if (image) {
        return encodeURI(BASE_URL + "/images/http://content.warframe.com/PublicExport" + image + "@png");
    }
    await updateImageDb();
    return encodeURI(BASE_URL + "/images/http://content.warframe.com/PublicExport" + await localforage.getItem(uniqueName) + "@png");
}