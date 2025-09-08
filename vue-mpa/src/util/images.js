import localforage from "localforage";
import { authFetch } from "./util";

localforage.config({
    name: 'warframe-tracker',
    storeName: 'images'
})

let updateImageDbPromise = null;
export const ManifestFetchStartedSignal = new Set();
export const ManifestFetchFinishedSignal = new Set();
export const ManifestParseStartedSignal = new Set();
export const ManifestParseFinishedSignal = new Set();
export const ManifestLoadStartedSignal = new Set();
export const ManifestLoadFinishedSignal = new Set();

export function subscribe(signal, callback) {
    signal.add(callback);
    return () => signal.delete(callback);
}

function emit(signal, args = null) {
    signal.forEach(cb => cb(args));
}

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
    const image = await localforage.getItem(uniqueName);
    if (image) {
        return encodeURI("https://www.localhost.me:18443/images/http://content.warframe.com/PublicExport" + image + "@png");
    }
    await updateImageDb();
    return encodeURI("https://www.localhost.me:18443/images/http://content.warframe.com/PublicExport" + await localforage.getItem(uniqueName) + "@png");
}