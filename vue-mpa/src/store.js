import Vuex from 'vuex';

export const store = new Vuex.Store({
    state: {
        username: null,
        token: null,
    },
    mutations: {
        setCredentials(state, { username, token }) {
            state.username = username;
            state.token = token;
            // Persist in localStorage
            localStorage.setItem('username', username);
            localStorage.setItem('token', token);
        },
        clearCredentials(state) {
            state.username = null;
            state.token = null;
            // Clear from localStorage
            localStorage.removeItem('username');
            localStorage.removeItem('token');
        },
    },
    actions: {
        initializeCredentials({ commit }) {
            // Check if credentials are saved in localStorage
            const savedUsername = localStorage.getItem('username');
            const savedToken = localStorage.getItem('token');

            if (savedUsername && savedToken) {
                commit('setCredentials', { username: savedUsername, token: savedToken });
            }
        },
    },
});

// Rehydrate the store when the application loads
store.dispatch('initializeCredentials');