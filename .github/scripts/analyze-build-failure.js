// analyze-build-failure.js — T-76/T-77/T-78
// GitHub Models API elemzés + OpenProject Bug WP létrehozás

const GITHUB_TOKEN     = process.env.GITHUB_TOKEN;
const OP_BASE_URL      = process.env.OP_BASE_URL;
const OP_API_TOKEN     = process.env.OP_API_TOKEN;
const OP_PROJECT_ID    = process.env.OP_PROJECT_ID;
const BRANCH           = process.env.BRANCH;
const COMMIT_SHA       = process.env.COMMIT_SHA;
const RUN_URL          = process.env.RUN_URL;
const COMMITTER_EMAIL  = process.env.COMMITTER_EMAIL;
const COMMITTER_NAME   = process.env.COMMITTER_NAME;

const MODEL = "gpt-4o";

async function analyzeFailure() {
    const shortSha = COMMIT_SHA?.substring(0, 7) ?? 'unknown';
    const prompt = `CI/CD build failure analyzer vagy egy ASP.NET Core 10 projekthez (Vertical Slice Architecture, EF Core 10, PostgreSQL, Wolverine).

Adatok:
- Branch: ${BRANCH}
- Commit: ${shortSha}
- Committer: ${COMMITTER_NAME} (${COMMITTER_EMAIL})
- Teljes build log: ${RUN_URL}

Generálj rövid, strukturált hibajelentést:
1. **Valószínű ok:** (1-2 mondat, a branch neve alapján következtess)
2. **Érintett terület:** (a branch névből következtess)
3. **Javasolt következő lépés:** (1 mondat)

Válaszolj magyarul, tömören.`;

    const response = await fetch("https://models.inference.ai.azure.com/chat/completions", {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${GITHUB_TOKEN}`,
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            model: MODEL,
            messages: [{ role: "user", content: prompt }],
            temperature: 0.3,
            max_tokens: 512,
        }),
    });

    if (!response.ok) throw new Error(`GitHub Models API hiba: ${response.status}`);
    const data = await response.json();
    return data.choices[0].message.content;
}

const opHeaders = () => {
    const credentials = `apikey:${OP_API_TOKEN}`;
    return {
        'Authorization': `Basic ${Buffer.from(credentials).toString('base64')}`,
        'Content-Type': 'application/json',
    };
};

async function getTypeId(name) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/types`, { headers: opHeaders() });
    const data = await res.json();
    return data._embedded.elements.find(t => t.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

async function getPriorityId(name) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/priorities`, { headers: opHeaders() });
    const data = await res.json();
    return data._embedded.elements.find(p => p.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

async function findUserId(email) {
    const encoded = encodeURIComponent(JSON.stringify([{"login":{"operator":"~","values":[email]}}]));
    const res = await fetch(`${OP_BASE_URL}/api/v3/users?filters=${encoded}`, { headers: opHeaders() });
    const data = await res.json();
    return data._embedded?.elements?.[0]?.id ?? null;
}

async function createBugWp(analysis) {
    const shortSha = COMMIT_SHA?.substring(0, 7) ?? 'unknown';
    const subject = `[CI FAIL] ${BRANCH} — build sikertelen (${shortSha})`;
    const description = `**AI-generált elemzés:**\n\n${analysis}\n\n---\n**Build log:** ${RUN_URL}\n**Committer:** ${COMMITTER_NAME} (${COMMITTER_EMAIL})\n**Commit:** ${shortSha}`;

    const [typeId, priorityId, assigneeId] = await Promise.all([
        getTypeId('Bug'),
        getPriorityId('High'),
        findUserId(COMMITTER_EMAIL),
    ]);

    const body = {
        subject,
        description: { raw: description },
        _links: {
            project: { href: `/api/v3/projects/${OP_PROJECT_ID}` },
            ...(typeId     && { type:     { href: `/api/v3/types/${typeId}` } }),
            ...(priorityId && { priority: { href: `/api/v3/priorities/${priorityId}` } }),
            ...(assigneeId && { assignee: { href: `/api/v3/users/${assigneeId}` } }),
        },
    };

    const res = await fetch(`${OP_BASE_URL}/api/v3/projects/${OP_PROJECT_ID}/work_packages`, {
        method: 'POST',
        headers: opHeaders(),
        body: JSON.stringify(body),
    });

    if (!res.ok) throw new Error(`OP Bug WP létrehozás hiba: ${res.status}`);
    const wp = await res.json();
    const wpId = Number(wp.id);
    console.log(`✅ Bug WP létrehozva, ID: ${wpId}`);
}

try {
    console.log('Build failure elemzés...');
    const analysis = await analyzeFailure();
    console.log('OP Bug WP létrehozása...');
    await createBugWp(analysis);
    console.log('Kész.');
} catch {
    console.error('⚠️ Build failure analysis sikertelen (nem kritikus).');
    process.exit(0);
}