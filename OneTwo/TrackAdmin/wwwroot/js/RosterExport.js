function PrintRosterGrid() {
    window.print();
}

function ExportRosterTableToCSV(filename) {
    var table = document.getElementById("roster-schedule-table");
    if (!table) return;

    var csv = [];
    var rows = table.querySelectorAll("tr");

    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll("td, th");

        for (var j = 0; j < cols.length; j++) {
            // Clean up text content of each cell
            var cellText = "";

            if (cols[j].classList.contains("rider-info-cell")) {
                // Get rider name and ID specifically
                var nameEl = cols[j].querySelector(".rider-name");
                var codeEl = cols[j].querySelector(".rider-code");
                var name = nameEl ? nameEl.textContent.trim() : "";
                var code = codeEl ? codeEl.textContent.trim() : "";
                cellText = name + " (" + code + ")";
            } else if (cols[j].querySelector(".assignment-card")) {
                // Get assignment details
                var slotEl = cols[j].querySelector(".slot-name");
                var dutyEl = cols[j].querySelector(".duty-badge");
                var slot = slotEl ? slotEl.textContent.trim() : "";
                var duty = dutyEl ? dutyEl.textContent.trim() : "";
                cellText = slot + " [" + duty + "]";
            } else if (cols[j].querySelector(".assign-placeholder-btn")) {
                cellText = "Unassigned";
            } else {
                cellText = cols[j].textContent.trim();
            }

            // Remove double quotes and escape quotes
            cellText = cellText.replace(/"/g, '""');
            // Wrap in double quotes to handle commas
            row.push('"' + cellText + '"');
        }

        csv.push(row.join(","));
    }

    // Download CSV
    var csvContent = "data:text/csv;charset=utf-8,\uFEFF" + csv.join("\n");
    var encodedUri = encodeURI(csvContent);
    var link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", filename);
    document.body.appendChild(link); // Required for FF
    link.click();
    document.body.removeChild(link);
}

function DownloadFile(filename, contentType, content) {
    const file = new Blob([content], { type: contentType });
    const exportUrl = URL.createObjectURL(file);
    const a = document.createElement("a");
    a.href = exportUrl;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(exportUrl);
}

function DownloadFileAsExcel(filename, sheetName, csvContent) {
    // Read the CSV data using SheetJS
    var lines = csvContent.split('\n');
    var data = lines.map(line => {
        // Parse CSV fields correctly (taking quotes into account)
        var row = [];
        var inQuotes = false;
        var currentField = "";
        for (var i = 0; i < line.length; i++) {
            var char = line[i];
            if (char === '"') {
                if (inQuotes && line[i+1] === '"') {
                    currentField += '"';
                    i++; // Skip double quote escape
                } else {
                    inQuotes = !inQuotes;
                }
            } else if (char === ',' && !inQuotes) {
                row.push(currentField);
                currentField = "";
            } else {
                currentField += char;
            }
        }
        row.push(currentField);
        return row;
    });

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(data);
    XLSX.utils.book_append_sheet(wb, ws, sheetName);
    XLSX.writeFile(wb, filename);
}
