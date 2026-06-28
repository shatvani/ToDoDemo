// AI Smoke Test — GitHub Models API
// Env változók (cd-staging.yml-ből):
//   GITHUB_TOKEN, REPO, COMMIT_SHA
//   HEALTH_STATUS, HEALTH_TIME, HEALTH_BODY
//   TODOS_STATUS, TODOS_TIME

import { Octokit } from "@octokit/rest";
import { callGhModels } from "./gh-models.js";

const GITHUB_TOKEN = process.env.GITHUB_TOKEN;
const REPO         = process.env.REPO;
const COMMIT_SHA   = process.env.COMMIT_SHA;

const HEALTH_STATUS = process.env.HEALTH_STATUS;
const HEALTH_TIME   = process.env.HEALTH_TIME;
const HEALTH_BODY   = process.env.HEALTH_BODY;
const TODOS_STATUS  = process.env.TODOS_STATUS;
const TODOS_TIME    = process.env.TODOS_TIME;

const [owner, repo] = REPO.split("/");
const octokit = new Octokit({ auth: GITHUB_TOKEN });
const MODEL = "gpt-4o";

async function generateSummary() {
    const testResults = `
Végpont tesztek:
- GET /api/health → HTTP ${HEALTH_STATUS}, ${HEALTH_TIME}s, válasz: ${HEALTH_BODY}
- GET /api/todos  → HTTP ${TODOS_STATUS}, ${TODOS_TIME}s
`;

    return callGhModels(GITHUB_TOKEN, {
        model: MODEL,
        messages: [
            {
                role: "system",
                content: `Te egy DevOps mérnök vagy, aki staging deploy után smoke test eredményeket értékel.
Az alkalmazás: ASP.NET Core 10 Minimal API Todo alkalmazás.
Értékeld az eredményeket tömören, max 5 mondatban, magyarul.
Jelezd: sikeres-e a deploy, van-e aggasztó jel.`,
            },
            {
                role: "user",
                content: testResults,
            },
        ],
        temperature: 0.2,
        max_tokens: 512,
    });
}

async function postCommitComment(summary) {
    const healthIcon = HEALTH_STATUS === "200" ? "✅" : "❌";
    const todosIcon  = TODOS_STATUS  === "200" ? "✅" : "❌";

    const body = `## 🚀 Staging Deploy — Smoke Test

| Végpont | Státusz | Válaszidő |
|---|---|---|
| ${healthIcon} \`GET /api/health\` | HTTP ${HEALTH_STATUS} | ${HEALTH_TIME}s |
| ${todosIcon} \`GET /api/todos\` | HTTP ${TODOS_STATUS} | ${TODOS_TIME}s |

### 🤖 AI Értékelés (GitHub Models / ${MODEL})

${summary}

---
_Automatikusan generálva a CD pipeline által._`;

    await octokit.repos.createCommitComment({
        owner,
        repo,
        commit_sha: COMMIT_SHA,
        body,
    });
}

async function main() {
    console.log("AI Smoke Test Summary generálása...");
    const summary = await generateSummary();
    console.log("Commit komment közzététele...");
    await postCommitComment(summary);
    console.log("Kész.");
}

await main().catch((err) => {
    console.error("Hiba:", err.message ?? err);
    process.exit(1);
});