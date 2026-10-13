(function () {
    const form = document.getElementById("urgent-tickets-filter-form");
    if (form) {
        form.querySelectorAll("[data-auto-submit]").forEach(function (select) {
            select.addEventListener("change", function () {
                form.submit();
            });
        });
    }

    const table = document.getElementById("urgent-tickets-table");
    if (!table) {
        return;
    }

    const storageKey = "soporte.urgentTickets.columnWidths";
    const minWidth = 56;
    const cols = Array.from(table.querySelectorAll("colgroup col"));
    const tbody = table.tBodies[0];

    function readWidths() {
        try {
            const raw = localStorage.getItem(storageKey);
            return raw ? JSON.parse(raw) : null;
        } catch {
            return null;
        }
    }

    function saveWidths() {
        const widths = {};
        cols.forEach(function (col) {
            if (col.dataset.col) {
                widths[col.dataset.col] = col.style.width || getComputedStyle(col).width;
            }
        });
        localStorage.setItem(storageKey, JSON.stringify(widths));
    }

    function applyWidths(widths) {
        cols.forEach(function (col) {
            const width = widths[col.dataset.col];
            if (width) {
                col.style.width = width;
            }
        });
    }

    const saved = readWidths();
    if (saved) {
        applyWidths(saved);
    }

    table.querySelectorAll(".urgent-col-resizer").forEach(function (resizer) {
        const th = resizer.closest("th");
        if (!th) {
            return;
        }

        const col = cols[th.cellIndex];
        if (!col) {
            return;
        }

        let startX = 0;
        let startWidth = 0;

        function onPointerMove(event) {
            const nextWidth = Math.max(minWidth, startWidth + (event.clientX - startX));
            col.style.width = nextWidth + "px";
        }

        function onPointerUp() {
            document.removeEventListener("pointermove", onPointerMove);
            document.removeEventListener("pointerup", onPointerUp);
            document.body.classList.remove("urgent-col-resizing");
            resizer.classList.remove("is-active");
            saveWidths();
        }

        resizer.addEventListener("pointerdown", function (event) {
            if (event.button !== undefined && event.button !== 0) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            startX = event.clientX;
            startWidth = th.getBoundingClientRect().width;
            document.body.classList.add("urgent-col-resizing");
            resizer.classList.add("is-active");
            document.addEventListener("pointermove", onPointerMove);
            document.addEventListener("pointerup", onPointerUp);
        });

        resizer.addEventListener("keydown", function (event) {
            if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") {
                return;
            }

            event.preventDefault();
            const current = th.getBoundingClientRect().width;
            const delta = event.key === "ArrowRight" ? 12 : -12;
            col.style.width = Math.max(minWidth, current + delta) + "px";
            saveWidths();
        });
    });

    function cellValue(row, index, type) {
        const cell = row.cells[index];
        if (!cell) {
            return type === "text" ? "" : 0;
        }

        const raw = (cell.dataset.sortValue ?? cell.textContent).trim();
        if (type === "number") {
            return Number(raw) || 0;
        }

        if (type === "date") {
            const parsed = Date.parse(raw);
            return Number.isNaN(parsed) ? 0 : parsed;
        }

        return raw.toLocaleLowerCase("es");
    }

    function compare(left, right, type, direction) {
        let result = 0;
        if (type === "text") {
            result = String(left).localeCompare(String(right), "es", { sensitivity: "base" });
        } else {
            result = left === right ? 0 : left < right ? -1 : 1;
        }

        return direction === "desc" ? -result : result;
    }

    table.querySelectorAll(".urgent-th-sort").forEach(function (button) {
        button.addEventListener("click", function () {
            const th = button.closest("th");
            if (!th || !tbody) {
                return;
            }

            const type = button.dataset.sortType || "text";
            const nextDir = th.dataset.sortDir === "asc" ? "desc" : "asc";

            table.querySelectorAll("thead th").forEach(function (header) {
                header.classList.remove("is-sorted");
                delete header.dataset.sortDir;
            });

            th.classList.add("is-sorted");
            th.dataset.sortDir = nextDir;

            const index = th.cellIndex;
            const rows = Array.from(tbody.rows);
            rows.sort(function (a, b) {
                return compare(cellValue(a, index, type), cellValue(b, index, type), type, nextDir);
            });
            rows.forEach(function (row) {
                tbody.appendChild(row);
            });
        });
    });
})();
