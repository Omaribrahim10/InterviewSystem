document.addEventListener("contextmenu", e => e.preventDefault());

document.addEventListener("keydown", function (e) {
    if (
        e.key === "F12" ||
        (e.ctrlKey && e.shiftKey && ["I", "J", "C"].includes(e.key.toUpperCase())) ||
        (e.ctrlKey && e.key.toUpperCase() === "U")
    ) {
        e.preventDefault();
    }
});

document.addEventListener("selectstart", e => e.preventDefault());
document.addEventListener("dragstart", e => e.preventDefault());

let devtoolsOpened = false;
const detectDevTools = () => {
    const before = new Date();
    debugger;
    const after = new Date();
    if (after - before > 100) {
        devtoolsOpened = true;
        document.body.innerHTML = `
    <div style="text-align:center;margin-top:20%;color:red;font-size:2rem;">
        ⚠️ Developer Tools Detected<br>Access Blocked.
    </div>`;
    }
};
setInterval(detectDevTools, 1000);