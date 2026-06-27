// OpenProject WP Komment — T-74
// Env változók: OP_BASE_URL, OP_API_TOKEN, WP_ID,
//               EVENT_TYPE, BRANCH, COMMIT_SHA, LOG_EXCERPT

const OP_BASE_URL  = process.env.OP_BASE_URL;
const OP_API_TOKEN = process.env.OP_API_TOKEN;
const WP_ID        = process.env.WP_ID;
const EVENT_TYPE   = process.env.EVENT_TYPE;
const BRANCH       = process.env.BRANCH;
const COMMIT_SHA   = process.env.COMMIT_SHA;
const LOG_EXCERPT  = process.env.LOG_EXCERPT || '';

if (!OP_BASE_URL || !OP_API_TOKEN || !WP_ID) {
    console.error("Hiányzó env változó: OP_BASE_URL, OP_API_TOKEN vagy WP_ID");
    process.exit(0);
}

function buildCommentText() {
    const shortSha = COMMIT_SHA ? COMMIT_SHA.substring(0, 7) : 'unknown';
    switch (EVENT_TYPE) {
        case 'build_passed':
            return `✅ **CI build sikeres**\n\n- Branch: \`${BRANCH}\`\n- Commit: \`${shortSha}\``;
        case 'build_failed':
            return `❌ **CI build sikertelen**\n\n- Branch: \`${BRANCH}\`\n- Commit: \`${shortSha}\`${LOG_EXCERPT ? `\n\n**Hibalog:**\n\`\`\`\n${LOG_EXCERPT}\n\`\`\`` : ''}`;
        case 'deployed':
            return `🚀 **Staging deploy sikeres**\n\n- Branch: \`${BRANCH}\`\n- Commit: \`${shortSha}\``;
        case 'deploy_failed':
            return `💥 **Staging deploy sikertelen**\n\n- Branch: \`${BRANCH}\`\n- Commit: \`${shortSha}\``;
        default:
            return `ℹ️ Pipeline esemény: ${EVENT_TYPE} — branch: \`${BRANCH}\`, commit: \`${shortSha}\``;
    }
}

async function postComment(wpId, commentText) {
    const auth = Buffer.from(`apikey:${OP_API_TOKEN}`).toString('base64');
    const url = `${OP_BASE_URL}/api/v3/work_packages/${wpId}/activities`;

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Basic ${auth}`,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ comment: { raw: commentText } }),
    });

    if (!response.ok) {
        throw new Error(`OpenProject API hiba: ${response.status} ${await response.text()}`);
    }

    console.log(`✅ Komment elküldve: WP #${wpId}`);
}

async function main() {
    const commentText = buildCommentText();
    console.log(`WP #${WP_ID} kommentálása (${EVENT_TYPE})...`);
    await postComment(WP_ID, commentText);
    console.log('Kész.');
}

main().catch((err) => {
    console.error('⚠️ OP komment sikertelen (nem kritikus):', err.message);
    process.exit(0);
});