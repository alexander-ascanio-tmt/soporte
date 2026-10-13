(function () {
    const preset = document.getElementById("period-preset");
    const dateFrom = document.getElementById("date-from");
    const dateTo = document.getElementById("date-to");

    function formatDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return year + "-" + month + "-" + day;
    }

    function applyPreset() {
        if (!preset || !dateFrom || !dateTo || !preset.value) {
            return;
        }

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const from = new Date(today);

        switch (preset.value) {
            case "3d":
                from.setDate(today.getDate() - 3);
                break;
            case "5d":
                from.setDate(today.getDate() - 5);
                break;
            case "1w":
                from.setDate(today.getDate() - 7);
                break;
            case "1m":
                from.setMonth(today.getMonth() - 1);
                break;
            case "3m":
                from.setMonth(today.getMonth() - 3);
                break;
            case "6m":
                from.setMonth(today.getMonth() - 6);
                break;
            case "1y":
                from.setFullYear(today.getFullYear() - 1);
                break;
            default:
                return;
        }

        dateFrom.value = formatDate(from);
        dateTo.value = formatDate(today);
    }

    if (preset) {
        preset.addEventListener("change", applyPreset);
        if (preset.value && (!dateFrom.value || !dateTo.value)) {
            applyPreset();
        }
    }

    if (dateFrom && dateTo && preset) {
        function markCustom() {
            preset.value = "";
        }

        dateFrom.addEventListener("change", markCustom);
        dateTo.addEventListener("change", markCustom);
    }

    const table = document.getElementById("clientResults");
    const resultsPanel = document.getElementById("client-results-panel");
    const selectedId = document.getElementById("selected-client-id");
    const selectedName = document.getElementById("selected-client-name");
    const fetchButton = document.getElementById("fetch-summary");

    function selectClient(id, name) {
        if (selectedId) {
            selectedId.value = id;
        }
        if (selectedName) {
            selectedName.value = name;
        }
        if (fetchButton) {
            fetchButton.disabled = !id;
        }
        if (resultsPanel) {
            resultsPanel.replaceChildren();
        }
    }

    if (table) {
        table.querySelectorAll(".register-result-row").forEach(function (row) {
            function applySelection() {
                selectClient(row.dataset.clientId || "", row.dataset.clientName || "");
            }

            row.addEventListener("click", applySelection);
            row.addEventListener("keydown", function (event) {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    applySelection();
                }
            });
        });
    }

    const copyButton = document.getElementById("copy-moneypenny");
    const copyLabel = document.getElementById("copy-moneypenny-label");
    const payload = document.getElementById("moneypenny-payload");
    if (copyButton && payload) {
        copyButton.addEventListener("click", async function () {
            const text = payload.value.trim();
            if (!text) {
                return;
            }

            try {
                await navigator.clipboard.writeText(text);
                copyButton.classList.add("is-copied");
                if (copyLabel) {
                    copyLabel.textContent = "Copiado";
                }
                setTimeout(function () {
                    copyButton.classList.remove("is-copied");
                    if (copyLabel) {
                        copyLabel.textContent = "Copiar";
                    }
                }, 1600);
            } catch {
                payload.select();
                document.execCommand("copy");
            }
        });
    }
})();
