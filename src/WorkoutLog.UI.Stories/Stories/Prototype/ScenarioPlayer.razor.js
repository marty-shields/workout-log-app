// Runs one scenario step inside the prototype's phone. Stories only.
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Waits for the element to render; null if it doesn't appear in time.
async function find(root, testId, timeout = 3000) {
    const end = Date.now() + timeout;
    for (;;) {
        const element = root.querySelector(`[data-testid="${CSS.escape(testId)}"]`);
        if (element || Date.now() > end) {
            return element;
        }
        await delay(50);
    }
}

async function outline(element, ms) {
    element.scrollIntoView({ block: "nearest" });
    const { outline, outlineOffset } = element.style;
    element.style.outline = "3px solid #1e88e5";
    element.style.outlineOffset = "2px";
    await delay(ms);
    element.style.outline = outline;
    element.style.outlineOffset = outlineOffset;
}

function input(element, value) {
    element.value = value;
    element.dispatchEvent(new Event("input", { bubbles: true }));
}

// Returns false if the target isn't found.
export async function run(root, action, testId, text) {
    const element = await find(root, testId);
    if (!element) {
        return false;
    }

    if (action === "Show") {
        await outline(element, 1500);
        return true;
    }

    await outline(element, 500);
    if (action === "Click") {
        element.click();
    } else if (action === "Enter") {
        input(element, "");
        for (const character of text) {
            await delay(80);
            input(element, element.value + character);
        }
    }

    return true;
}
