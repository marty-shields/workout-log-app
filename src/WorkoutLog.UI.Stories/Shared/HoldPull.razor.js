// Sets what AppScreen.razor.js sets while a finger pulls: the ring three quarters of the way to the threshold.
export function hold() {
    const indicator = document.querySelector("[data-testid=pull-refresh]");
    const threshold = parseFloat(getComputedStyle(indicator).getPropertyValue("--pull-refresh-threshold"));
    indicator.dataset.pull = "";
    indicator.style.setProperty("--pull", `${threshold * 0.75}px`);
}
