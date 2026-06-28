// AI Code Review — GitHub Models API
// Környezeti változók (ci.yml-ből):
//   GITHUB_TOKEN  — PR komment írásához és diff letöltéséhez
//   PR_NUMBER     — a review-zandó pull request száma
//   REPO          — "owner/repo" formátum

import { Octokit } from "@octokit/rest";
import { callGhModels } from "./gh-models.js";

const GITHUB_TOKEN = process.env.GITHUB_TOKEN;
const PR_NUMBER    = Number.parseInt(process.env.PR_NUMBER, 10);
const REPO         = process.env.REPO; // "owner/repo"

if (!GITHUB_TOKEN || !PR_NUMBER || !REPO) {
    console.error("Hiányzó környezeti változó: GITHUB_TOKEN, PR_NUMBER, REPO");
    process.exit(1);
}

const [owner, repo] = REPO.split("/");

const octokit = new Octokit({ auth: GITHUB_TOKEN });

const MODEL = "gpt-4o";
const MAX_DIFF_CHARS = 24000; // ~6k token, biztonságos határ

async function getPrDiff() {
    const { data } = await octokit.pulls.get({
        owner,
        repo,
        pull_number: PR_NUMBER,
        mediaType: { format: "diff" },
    });
    // Az Octokit diff formátumban stringként adja vissza
    return typeof data === "string" ? data : JSON.stringify(data);
}

async function getPrMetadata() {
    const { data } = await octokit.pulls.get({ owner, repo, pull_number: PR_NUMBER });
    return {
        title: data.title,
        body:  data.body ?? "",
        base:  data.base.ref,
        head:  data.head.ref,
    };
}

async function runAiReview(diff, pr) {
    const truncatedDiff = diff.length > MAX_DIFF_CHARS
        ? diff.slice(0, MAX_DIFF_CHARS) + "\n\n[... diff csonkítva a token limit miatt ...]"
        : diff;

    const systemPrompt = `Te egy szigorú, de konstruktív .NET / C# code reviewer vagy.
A projekt egy ASP.NET Core 10 Minimal API + Razor Pages alkalmazás, Vertical Slice Architecture (VSA) szerint szervezve.
Wolverine HTTP handlerek, FluentValidation, EF Core 10 + PostgreSQL, HTMX frontend.

Fókuszterületek (fontossági sorrendben):
1. Biztonsági problémák (injection, auth bypass, SSRF stb.)
2. Helyességi hibák (logikai hibák, race condition, null ref)
3. VSA konvenciók betartása (nincs Controller, MediatR, AutoMapper, Repository)
4. C# / .NET best practice (nullable, async/await, using, cancellation token)
5. Teljesítmény (N+1 lekérdezés, felesleges allokáció)
6. Kód olvashatóság (elnevezések, komplexitás)

Válaszod formátuma KIZÁRÓLAG Markdown legyen az alábbi szekcióstruktúrával.
Ha egy szekció üres (nincs találat), hagyd el teljesen.`;

    const userPrompt = `## PR információ
**Cím:** ${pr.title}
**Branch:** ${pr.head} → ${pr.base}
**Leírás:**
${pr.body || "_nincs leírás_"}

## Diff
\`\`\`diff
${truncatedDiff}
\`\`\`

Kérlek végezd el a code review-t a megadott szempontok szerint.`;

    return callGhModels(GITHUB_TOKEN, {
        model: MODEL,
        messages: [
            { role: "system", content: systemPrompt },
            { role: "user",   content: userPrompt },
        ],
        temperature: 0.2,
        max_tokens: 2048,
    });
}

async function postReviewComment(body) {
    await octokit.issues.createComment({
        owner,
        repo,
        issue_number: PR_NUMBER,
        body: `## 🤖 AI Code Review (GitHub Models / ${MODEL})\n\n${body}\n\n---\n_Ez a review automatikusan készült. Emberi felülvizsgálat ajánlott._`,
    });
}

async function main() {
    console.log(`AI Code Review — PR #${PR_NUMBER} (${REPO})`);

    console.log("Diff letöltése...");
    const diff = await getPrDiff();
    if (!diff || diff.trim().length === 0) {
        console.log("Üres diff, review kihagyva.");
        return;
    }
    console.log(`Diff mérete: ${diff.length} karakter`);

    console.log("PR metaadatok lekérése...");
    const pr = await getPrMetadata();

    console.log(`AI review futtatása (${MODEL})...`);
    const reviewText = await runAiReview(diff, pr);

    console.log("Komment közzététele...");
    await postReviewComment(reviewText);

    console.log("Kész.");
}

await main().catch((err) => {
    console.error("Hiba:", err.message ?? err);
    process.exit(1);
});
