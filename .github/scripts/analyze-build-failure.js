// analyze-build-failure.js — T-76/T-77/T-78
// GitHub Models API elemzés + OpenProject Bug WP létrehozás

import { callGhModels } from "./gh-models.js";
import { opHeaders, getTypeId, getPriorityId, findUserId } from "./op-api.js";

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

    return callGhModels(GITHUB_TOKEN, {
        model: MODEL,
        messages: [{ role: "user", content: prompt }],
        temperature: 0.3,
        max_tokens: 512,
    });
}

async function createBugWp(analysis) {
    const shortSha = COMMIT_SHA?.substring(0, 7) ?? 'unknown';
    const subject = `[CI FAIL] ${BRANCH} — build sikertelen (${shortSha})`;
    const description = `**AI-generált elemzés:**\n\n${analysis}\n\n---\n**Build log:** ${RUN_URL}\n**Committer:** ${COMMITTER_NAME} (${COMMITTER_EMAIL})\n**Commit:** ${shortSha}`;

    const [typeId, priorityId, assigneeId] = await Promise.all([
        getTypeId(OP_BASE_URL, OP_API_TOKEN, 'Bug'),
        getPriorityId(OP_BASE_URL, OP_API_TOKEN, 'High'),
        findUserId(OP_BASE_URL, OP_API_TOKEN, COMMITTER_EMAIL),
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
        headers: opHeaders(OP_API_TOKEN),
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