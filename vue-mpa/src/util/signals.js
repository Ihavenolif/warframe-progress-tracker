export const ManifestFetchStartedSignal = new Set();
export const ManifestFetchFinishedSignal = new Set();
export const ManifestParseStartedSignal = new Set();
export const ManifestParseFinishedSignal = new Set();
export const ManifestLoadStartedSignal = new Set();
export const ManifestLoadFinishedSignal = new Set();
export const TokenUpdateSignal = new Set();

export function subscribe(signal, callback) {
    signal.add(callback);
    return () => signal.delete(callback);
}

export function emit(signal, args = null) {
    signal.forEach(cb => cb(args));
}