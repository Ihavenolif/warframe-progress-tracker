import http from 'k6/http';
import { check, fail } from 'k6';

const baseUrl = (__ENV.BASE_URL || 'http://localhost:5224').replace(/\/$/, '');
const query = __ENV.QUERY || 'owned=all&sort=name&page=1&pageSize=20';

export const options = {
    scenarios: {
        relics: {
            executor: 'constant-vus',
            vus: Number(__ENV.VUS || 5),
            duration: __ENV.DURATION || '30s',
        },
    },
    thresholds: {
        checks: ['rate>0.99'],
        http_req_failed: ['rate<0.01'],
    },
};

export function setup() {
    if (__ENV.ACCESS_TOKEN) {
        return { token: __ENV.ACCESS_TOKEN };
    }

    if (!__ENV.USERNAME || !__ENV.PASSWORD) {
        fail('Set ACCESS_TOKEN, or both USERNAME and PASSWORD.');
    }

    const loginUrl =
        `${baseUrl}/api/auth/login` +
        `?username=${encodeURIComponent(__ENV.USERNAME)}` +
        `&password=${encodeURIComponent(__ENV.PASSWORD)}`;
    const response = http.post(loginUrl, null, {
        tags: { endpoint: 'auth-login' },
    });

    if (!check(response, { 'login returned 200': result => result.status === 200 })) {
        fail(`Login failed: HTTP ${response.status} ${response.body}`);
    }
    return { token: response.json('token') };
}

export default function (data) {
    const response = http.get(`${baseUrl}/api/relics?${query}`, {
        headers: {
            Authorization: `Bearer ${data.token}`,
            Accept: 'application/json',
        },
        tags: { endpoint: 'GET /api/relics' },
    });

    let body = null;
    try {
        body = response.json();
    } catch {
        // Shape check below reports non-JSON responses.
    }

    check(response, {
        'status is 200': result => result.status === 200,
        'response is relic page': () =>
            body !== null &&
            Array.isArray(body.items) &&
            Number.isInteger(body.page) &&
            Number.isInteger(body.pageSize) &&
            Number.isInteger(body.totalCount) &&
            Number.isInteger(body.totalPages),
    });
}
