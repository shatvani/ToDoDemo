// po-task-assistant.js — T-79–T-82
// User Story-k alá AI-generált Task-ok létrehozása (B megközelítés: polling)

const GITHUB_TOKEN  = process.env.GITHUB_TOKEN;
const OP_BASE_URL   = process.env.OP_BASE_URL;
const OP_API_TOKEN  = process.env.OP_API_TOKEN;
const OP_PROJECT_ID = process.env.OP_PROJECT_ID;

const MODEL = "gpt-4o";

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

async function getStatusId(name) {
    const res = await fetch(`${OP_BASE_URL}/api/v3/statuses`, { headers: opHeaders() });
    const data = await res.json();
    return data._embedded.elements.find(s => s.name.toLowerCase() === name.toLowerCase())?.id ?? null;
}

async function getUserStories(typeId, statusId) {
    const filters = JSON.stringify([
        { "type": { "operator": "=", "values": [String(typeId)] } },
        { "status": { "operator": "=", "values": [String(statusId)] } },
    ]);
    const res = await fetch(
        `${OP_BASE_URL}/api/v3/projects/${OP_PROJECT_ID}/work_packages?filters=${encodeURIComponent(filters)}&pageSize=50`,
        { headers: opHeaders() }
    );
    const data = await res.json();
    return data._embedded?.elements ?? [];
}

async function hasChildren(wpId) {
    const res = await fetch(
        `${OP_BASE_URL}/api/v3/work_packages/${wpId}/children`,
        { headers: opHeaders() }
    );
    const data = await res.json();
    return (data._embedded?.elements?.length ?? 0) > 0;
}

async function generateTasks(subject, description) {
    const prompt = `Te egy tapasztalt szoftverfejlesztő csapat tech leadje vagy.
Egy User Story-hoz kell fejlesztési task-okat generálnod.

User Story: ${subject}
Leírás: ${description || '(nincs leírás)'}

Generálj 3-5 konkrét, elvégezhető fejlesztési task-ot ehhez a User Story-hoz.
Az alkalmazás: ASP.NET Core 10 Minimal API + EF Core + PostgreSQL + HTMX (Vertical Slice Architecture).

Válaszolj kizárólag JSON tömbben, ebben a formátumban:
[
  { "subject": "task neve", "description": "rövid leírás, mi a teendő" }
]

Csak a JSON tömböt add vissza, semmi mást.`;

    const response = await fetch("https://models.inference.ai.azure.com/chat/completions", {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${GITHUB_TOKEN}`,
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            model: MODEL,
            messages: [{ role: "user", content: prompt }],
            temperature: 0.4,
            max_tokens: 1024,
        }),
    });

    if (!response.ok) throw new Error(`GitHub Models API hiba: ${response.status}`);
    const data = await response.json();
    const content = data.choices[0].message.content.trim();
    const jsonMatch = content.match(/\[[\s\S]*\]/);
    if (!jsonMatch) throw new Error('AI nem adott vissza valid JSON tömböt');
    return JSON.parse(jsonMatch[0]);
}

async function createTask(parentId, taskTypeId, statusId, subject, description) {
    const taskSubject = `[AI] ${subject}`;
    const taskDescription = `${description}\n\n---\n_AI-generált task — felülvizsgálatra vár._`;
    const body = {
        subject: taskSubject,
        description: { raw: taskDescription },
        _links: {
            project:  { href: `/api/v3/projects/${OP_PROJECT_ID}` },
            type:     { href: `/api/v3/types/${taskTypeId}` },
            status:   { href: `/api/v3/statuses/${statusId}` },
            parent:   { href: `/api/v3/work_packages/${parentId}` },
        },
    };

    const res = await fetch(`${OP_BASE_URL}/api/v3/projects/${OP_PROJECT_ID}/work_packages`, {
        method: 'POST',
        headers: opHeaders(),
        body: JSON.stringify(body),
    });

    if (!res.ok) throw new Error(`Task létrehozás hiba: ${res.status}`);
    return res.json();
}

try {
    console.log('PO Task Assistant indul...');

    const [userStoryTypeId, taskTypeId, newStatusId] = await Promise.all([
        getTypeId('User Story'),
        getTypeId('Task'),
        getStatusId('New'),
    ]);

    if (!userStoryTypeId) throw new Error('User Story típus nem található');
    if (!taskTypeId) throw new Error('Task típus nem található');
    if (!newStatusId) throw new Error('New státusz nem található');

    const userStories = await getUserStories(userStoryTypeId, newStatusId);
    console.log(`${userStories.length} New státuszú User Story található.`);

    let processed = 0;
    // Szekvenciális feldolgozás: elkerüli az API rate limit problémákat
    for (const us of userStories) {
        const wpId = Number(us.id);

        const alreadyDone = await hasChildren(wpId);
        if (alreadyDone) {
            console.log(`WP #${wpId} már feldolgozott (van child WP), kihagyva.`);
            continue;
        }

        console.log(`WP #${wpId} feldolgozása...`);
        const description = us.description?.raw ?? '';

        let tasks;
        try {
            tasks = await generateTasks(us.subject, description);
        } catch {
            console.error(`WP #${wpId} AI generálás sikertelen, kihagyva.`);
            continue;
        }

        console.log(`  ${tasks.length} task generálva`);
        for (const task of tasks) {
            await createTask(wpId, taskTypeId, newStatusId, task.subject, task.description);
        }
        processed++;
        console.log(`  WP #${wpId} feldolgozva`);
    }

    console.log(`Kész. ${processed} User Story feldolgozva.`);
} catch {
    console.error('PO Task Assistant hiba (nem kritikus).');
    process.exit(0);
}