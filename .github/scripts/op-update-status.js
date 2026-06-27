// OpenProject WP Státusz frissítés — T-75
// Env változók: OP_BASE_URL, OP_API_TOKEN, WP_ID, STATUS_NAME

const OP_BASE_URL  = process.env.OP_BASE_URL;
const OP_API_TOKEN = process.env.OP_API_TOKEN;
const WP_ID        = process.env.WP_ID;
const STATUS_NAME  = process.env.STATUS_NAME || 'Closed';

if (!OP_BASE_URL || !OP_API_TOKEN || !WP_ID) {
    console.error("Hiányzó env változó: OP_BASE_URL, OP_API_TOKEN vagy WP_ID");
    process.exit(0);
}

const auth = Buffer.from(`apikey:${OP_API_TOKEN}`).toString('base64');
const headers = {
    'Authorization': `Basic ${auth}`,
    'Content-Type': 'application/json',
};

async function getWp(wpId) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/work_packages/${wpId}`, { headers });
    if (!res.ok) throw new Error(`WP lekérés hiba: ${res.status} ${await res.text()}`);
    return res.json();
}

async function getStatusIdByName(name) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/statuses`, { headers });
    if (!res.ok) throw new Error(`Státuszok lekérés hiba: ${res.status}`);
    const data = await res.json();
    const status = data._embedded.elements.find(s => s.name === name);
    if (!status) throw new Error(`Státusz nem található: "${name}"`);
    return status.id;
}

async function updateWpStatus(wpId, statusId, lockVersion) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/work_packages/${wpId}`, {
        method: 'PATCH',
        headers,
        body: JSON.stringify({
            lockVersion,
            status: { href: `/api/v3/statuses/${statusId}` },
        }),
    });
    if (!res.ok) throw new Error(`Státusz frissítés hiba: ${res.status} ${await res.text()}`);
    console.log(`✅ WP #${wpId} státusz frissítve → ${STATUS_NAME}`);
}

async function main() {
    console.log(`WP #${WP_ID} státusz frissítése → ${STATUS_NAME}...`);
    const wp = await getWp(WP_ID);
    const statusId = await getStatusIdByName(STATUS_NAME);
    await updateWpStatus(WP_ID, statusId, wp.lockVersion);
    console.log('Kész.');
}

main().catch(err => {
    console.error('⚠️ Státusz frissítés sikertelen (nem kritikus):', err.message);
    process.exit(0);
});