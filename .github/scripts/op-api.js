// op-api.js — OpenProject API kliens (shared utility)

export const opHeaders = (token) => {
    const credentials = `apikey:${token}`;
    return {
        'Authorization': `Basic ${Buffer.from(credentials).toString('base64')}`,
        'Content-Type': 'application/json',
    };
};

export async function getTypeId(baseUrl, token, name) {
    const res = await fetch(`${baseUrl}/api/v3/types`, { headers: opHeaders(token) });
    const data = await res.json();
    return data._embedded.elements.find(t => t.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

export async function getStatusId(baseUrl, token, name) {
    const res = await fetch(`${baseUrl}/api/v3/statuses`, { headers: opHeaders(token) });
    const data = await res.json();
    return data._embedded.elements.find(s => s.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

export async function getPriorityId(baseUrl, token, name) {
    const res = await fetch(`${baseUrl}/api/v3/priorities`, { headers: opHeaders(token) });
    const data = await res.json();
    return data._embedded.elements.find(p => p.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

export async function findUserId(baseUrl, token, email) {
    const encoded = encodeURIComponent(JSON.stringify([{ "login": { "operator": "~", "values": [email] } }]));
    const res = await fetch(`${baseUrl}/api/v3/users?filters=${encoded}`, { headers: opHeaders(token) });
    const data = await res.json();
    return data._embedded?.elements?.[0]?.id ?? null;
}