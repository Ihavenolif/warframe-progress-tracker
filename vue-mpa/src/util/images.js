import localforage from "localforage";
import { authFetch } from "./util";

localforage.config({
    name: 'warframe-tracker',
    storeName: 'images'
})

let updateImageDbPromise = null;

async function getIndex() {
    const res = await authFetch("/api/items/index");
    return await res.json();
}

async function updateImageDb() {
    if (updateImageDbPromise) {
        return updateImageDbPromise;
    }
    updateImageDbPromise = (async () => {
        const index = await getIndex();

        const res = await fetch("https://content.warframe.com/PublicExport/Manifest/" + index["Manifest"]);
        const imageLinks = (await res.json())["Manifest"]

        imageLinks.forEach(item => {
            localforage.setItem(item["uniqueName"], item["textureLocation"]);
        });

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