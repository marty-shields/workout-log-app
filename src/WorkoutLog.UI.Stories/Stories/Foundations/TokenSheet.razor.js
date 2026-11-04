// Reads token values for the token sheet.

// "rgb(48, 82, 82)", "rgba(0, 0, 0, 0.45)" or color-mix()'s "color(srgb 0.2 0.3 0.3 / 0.12)" to "#305252", "#00000073" or "#334d4d1f".
function hex(colour) {
    const scale = colour.startsWith("color(") ? 255 : 1;
    const [r, g, b, a] = colour.match(/[\d.]+/g).map(Number);
    const rgb = [r, g, b].map(n => Math.round(n * scale));
    const parts = a === undefined ? rgb : [...rgb, Math.round(a * 255)];
    return "#" + parts.map(n => n.toString(16).padStart(2, "0")).join("");
}

// Keys are "<token> <theme>" for colours, read from each swatch, and "<token>" for the rest.
export function read(sheet) {
    const values = {};
    for (const swatch of sheet.querySelectorAll(".swatch")) {
        const token = swatch.closest("[data-token]").dataset.token;
        const theme = swatch.closest("[data-theme]").dataset.theme;
        values[`${token} ${theme}`] = hex(getComputedStyle(swatch).backgroundColor);
    }

    const root = getComputedStyle(document.documentElement);
    for (const item of sheet.querySelectorAll("li[data-token]")) {
        values[item.dataset.token] = root.getPropertyValue(item.dataset.token).trim();
    }

    return values;
}
