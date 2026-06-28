// gh-models.js — GitHub Models API kliens (shared utility)

const MODELS_URL = "https://models.inference.ai.azure.com/chat/completions";
const RETRY_DELAY_MS = 62000;

export async function callGhModels(token, payload, retries = 1) {
    for (let attempt = 0; attempt <= retries; attempt++) {
        const response = await fetch(MODELS_URL, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json",
            },
            body: JSON.stringify(payload),
        });

        if (response.status === 429 && attempt < retries) {
            console.log('Rate limit, 62s várakozás...');
            await new Promise(r => setTimeout(r, RETRY_DELAY_MS));
            continue;
        }

        if (!response.ok) throw new Error(`GitHub Models API hiba: ${response.status}`);
        const data = await response.json();
        return data.choices[0].message.content;
    }
}