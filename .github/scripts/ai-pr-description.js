// AI PR Description — GitHub Models API
// Környezeti változók (ci.yml-ből):
//   GITHUB_TOKEN  — PR frissítéséhez és diff letöltéséhez
//   PR_NUMBER     — a pull request száma
//   REPO          — "owner/repo" formátum

import { Octokit } from "@octokit/rest";

const GITHUB_TOKEN = process.env.GITHUB_TOKEN;
const PR_NUMBER    = Number.parseInt(process.env.PR_NUMBER, 10);
const REPO         = process.env.REPO;

if (!GITHUB_TOKEN || !PR_NUMBER || !REPO) {
    console.error("Hiányzó környezeti változó: GITHUB_TOKEN, PR_NUMBER, REPO");
    process.exit(1);
}

const [owner, repo] = REPO.split("/");
const octokit = new Octokit({ auth: GITHUB_TOKEN });

const MODEL = "gpt-4o";
const MAX_DIFF_CHARS = 24000;

async function getPrMetadata() {
    const { data } = await octokit.pulls.get({ owner, repo, pull_number: PR_NUMBER });
    return {
        title:  data.title,
        body:   data.body ?? "",
        base:   data.base.ref,
        head:   data.head.ref,
    };
}

async function getPrDiff() {
    const { data } = await octokit.pulls.get({
        owner,
        repo,
        pull_number: PR_NUMBER,
        mediaType: { format: "diff" },
    });
    return typeof data === "string" ? data : JSON.stringify(data);
}

async function generateDescription(diff, pr) {
    const truncatedDiff = diff.length > MAX_DIFF_CHARS
        ? diff.slice(0, MAX_DIFF_CHARS) + "\n\n[... diff csonkítva a token limit miatt ...]"
        : diff;

    const systemPrompt = `Te egy tapasztalt .NET fejlesztő vagy, aki PR leírásokat ír.
A projekt: ASP.NET Core 10 Minimal API, Vertical Slice Architecture, EF Core 10 + PostgreSQL, HTMX frontend, Wolverine HTTP handlerek.

Generálj tömör, informatív PR leírást a következő Markdown struktúrával:

## Mit csinál ez a PR?
(1-3 mondatos összefoglaló)

## Változások
(felsorolás a legfontosabb módosításokról, max 8 pont)

## Tesztelés
(hogyan lehet ellenőrizni — unit tesztek, manuális lépések)

## Megjegyzések
(opcionális — breaking change, függőség, tudnivaló — ha nincs, hagyd el)

Szabályok:
- Légy tömör, kerüld a felesleges szót
- Csak érdemi változásokat sorolj fel
- Válaszolj **magyarul**`;

    const userPrompt = `## PR adatok
**Cím:** ${pr.title}
**Branch:** ${pr.head} → ${pr.base}

## Diff
\`\`\`diff
${truncatedDiff}
\`\`\`

Generáld a PR leírást.`;

    const response = await fetch("https://models.inference.ai.azure.com/chat/completions", {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${GITHUB_TOKEN}`,
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            model: MODEL,
            messages: [
                { role: "system", content: systemPrompt },
                { role: "user",   content: userPrompt },
            ],
            temperature: 0.3,
            max_tokens: 1024,
        }),
    });

    if (!response.ok) {
        throw new Error(`GitHub Models API hiba: ${response.status} ${await response.text()}`);
    }

    const data = await response.json();
    return data.choices[0].message.content;
}

async function updatePrDescription(body) {
    await octokit.pulls.update({
        owner,
        repo,
        pull_number: PR_NUMBER,
        body: `${body}\n\n---\n_Leírás automatikusan generálva: AI PR Description (GitHub Models / ${MODEL})_`,
    });
}

async function main() {
    console.log(`AI PR Description — PR #${PR_NUMBER} (${REPO})`);

    console.log("PR metaadatok lekérése...");
    const pr = await getPrMetadata();

    if (pr.body && pr.body.trim().length > 0) {
        console.log("PR leírás már létezik, kihagyva.");
        return;
    }

    console.log("Diff letöltése...");
    const diff = await getPrDiff();
    if (!diff || diff.trim().length === 0) {
        console.log("Üres diff, kihagyva.");
        return;
    }

    console.log(`AI leírás generálása (${MODEL})...`);
    const description = await generateDescription(diff, pr);

    console.log("PR leírás frissítése...");
    await updatePrDescription(description);

    console.log("Kész.");
}

await main().catch((err) => {
    console.error("Hiba:", err.message ?? err);
    process.exit(1);
});