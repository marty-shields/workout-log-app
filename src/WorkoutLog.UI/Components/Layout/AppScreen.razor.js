const gestures = new WeakMap();

const damping = 0.5;

export function attach(content, indicator, screen) {
    const threshold = parseFloat(getComputedStyle(indicator).getPropertyValue("--pull-refresh-threshold"));
    const maxPull = threshold * 1.5;
    let phase = "idle";
    let startY = 0;
    let pull = 0;

    const show = (next) => {
        phase = next;
        if (phase === "idle") {
            delete indicator.dataset.pull;
        } else {
            indicator.dataset.pull = phase;
        }

        if (phase === "pulling") {
            indicator.style.setProperty("--pull", `${pull}px`);
        } else {
            indicator.style.removeProperty("--pull");
        }
    };

    const start = (event) => {
        if (phase !== "idle" || content.scrollTop > 0 || event.touches.length !== 1) {
            return;
        }

        startY = event.touches[0].clientY;
        pull = 0;
        show("pulling");
    };

    const move = (event) => {
        if (phase !== "pulling") {
            return;
        }

        const distance = event.touches[0].clientY - startY;
        if (distance <= 0 && pull === 0) {
            show("idle");
            return;
        }

        event.preventDefault();
        pull = Math.min(Math.max(distance, 0) * damping, maxPull);
        show("pulling");
    };

    const end = async () => {
        if (phase !== "pulling") {
            return;
        }

        if (pull < threshold) {
            show("idle");
            return;
        }

        show("released");
        try {
            await screen.invokeMethodAsync("RefreshAsync");
        } finally {
            show("idle");
        }
    };

    content.addEventListener("touchstart", start, { passive: true });
    content.addEventListener("touchmove", move, { passive: false });
    content.addEventListener("touchend", end);
    content.addEventListener("touchcancel", end);
    gestures.set(content, { start, move, end });
}

export function detach(content) {
    const gesture = gestures.get(content);
    if (!gesture) {
        return;
    }

    content.removeEventListener("touchstart", gesture.start);
    content.removeEventListener("touchmove", gesture.move);
    content.removeEventListener("touchend", gesture.end);
    content.removeEventListener("touchcancel", gesture.end);
    gestures.delete(content);
}
