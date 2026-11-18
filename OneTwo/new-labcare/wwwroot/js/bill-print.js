/**
 * Bill Print Module for LabCare LIMS
 * Generates PDF from bill HTML template, converts to base64,
 * and sends to labcareprint:// protocol handler.
 */
(function () {
    'use strict';

    // ── Number to Words Converter ──────────────────────────────────
    const ones = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine',
        'Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen',
        'Seventeen', 'Eighteen', 'Nineteen'];
    const tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

    function numberToWords(num) {
        if (num === 0) return 'Zero';
        num = Math.round(num);
        if (num < 0) return 'Minus ' + numberToWords(-num);

        let words = '';
        if (Math.floor(num / 10000000) > 0) {
            words += numberToWords(Math.floor(num / 10000000)) + ' Crore ';
            num %= 10000000;
        }
        if (Math.floor(num / 100000) > 0) {
            words += numberToWords(Math.floor(num / 100000)) + ' Lakh ';
            num %= 100000;
        }
        if (Math.floor(num / 1000) > 0) {
            words += numberToWords(Math.floor(num / 1000)) + ' Thousand ';
            num %= 1000;
        }
        if (Math.floor(num / 100) > 0) {
            words += ones[Math.floor(num / 100)] + ' Hundred ';
            num %= 100;
        }
        if (num > 0) {
            if (words !== '') words += 'and ';
            if (num < 20) {
                words += ones[num];
            } else {
                words += tens[Math.floor(num / 10)];
                if (num % 10 > 0) words += ' ' + ones[num % 10];
            }
        }
        return words.trim();
    }

    function amountToWords(amount) {
        const rupees = Math.floor(amount);
        const paise = Math.round((amount - rupees) * 100);
        let result = numberToWords(rupees) + ' Rupees';
        if (paise > 0) result += ' and ' + numberToWords(paise) + ' Paise';
        return result + ' Only';
    }

    // ── Format Currency ────────────────────────────────────────────
    function formatCurrency(val) {
        return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // ── Build Bill HTML ────────────────────────────────────────────
    function buildBillHtml(billData) {
        const d = billData;
        const grossAmount = d.requestamount || d.totalamount || 0;
        const discountAmount = d.discountamount || 0;
        const totalAmount = d.totalamount || 0;
        const paidAmount = d.paidamount || 0;
        const balanceAmount = totalAmount - paidAmount;
        const billDate = d.requestdatetime ? new Date(d.requestdatetime).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) : '';
        const billTime = d.requestdatetime ? new Date(d.requestdatetime).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' }) : '';

        // Payment status
        const isPaid = balanceAmount <= 0;
        const statusText = isPaid ? 'PAID' : 'DUE';
        const statusColor = isPaid ? '#1e8e3e' : '#d93025';

        // Payment method
        let payMethod = 'Cash';
        if (d.pmc2 && d.pmc2 > 0 && d.pmc3 && d.pmc3 > 0) payMethod = 'Cash + Card + UPI';
        else if (d.pmc2 && d.pmc2 > 0) payMethod = 'Cash + Card';
        else if (d.pmc3 && d.pmc3 > 0) payMethod = 'Cash + UPI';

        // Build test items rows
        let itemsHtml = '';
        if (d.testItems && d.testItems.length > 0) {
            d.testItems.forEach((item, idx) => {
                const rate = (item.testamount || item.standardprice || item.testrate || 0);
                const disc = (item.discount || 0);
                const total = (item.collection || rate - disc);
                itemsHtml += `
                    <tr>
                        <td style="text-align: center; padding: 7px 10px; border-bottom: 1px solid #dadce0; font-size: 12px;">${idx + 1}</td>
                        <td style="padding: 7px 10px; border-bottom: 1px solid #dadce0; font-size: 12px;">${item.testname || item.description || 'Test ' + (idx + 1)}</td>
                        <td style="text-align: right; padding: 7px 10px; border-bottom: 1px solid #dadce0; font-size: 12px;">${rate.toFixed(2)}</td>
                        <td style="text-align: right; padding: 7px 10px; border-bottom: 1px solid #dadce0; font-size: 12px;">${disc.toFixed(2)}</td>
                        <td style="text-align: right; padding: 7px 10px; border-bottom: 1px solid #dadce0; font-size: 12px; font-weight: 600;">${total.toFixed(2)}</td>
                    </tr>`;
            });
        }

        // Build age string
        let ageStr = '';
        if (d.ageyears) ageStr += d.ageyears + 'Y';
        if (d.agemonths && d.agemonths !== '0') ageStr += ' ' + d.agemonths + 'M';
        if (d.agedays && d.agedays !== '0') ageStr += ' ' + d.agedays + 'D';
        if (d.gender) ageStr += ' / ' + d.gender;

        const balanceRowDisplay = balanceAmount > 0 ? 'flex' : 'none';

        return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Bill - ${d.requestsnoprint || ''}</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary: #1a73e8;
            --primary-dark: #1557b0;
            --primary-light: #e8f0fe;
            --text-main: #202124;
            --text-secondary: #5f6368;
            --border: #dadce0;
            --white: #ffffff;
            --danger: #d93025;
            --success: #1e8e3e;
        }
        @page { size: 80mm auto; margin: 0; }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: var(--white);
            color: var(--text-main);
            line-height: 1.4;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
            font-size: 10px;
        }
        .document-wrapper {
            width: 80mm;
            max-width: 80mm;
            margin: 0 auto;
            background: var(--white);
            padding: 4mm 5mm;
            position: relative;
        }
        header {
            text-align: center;
            border-bottom: 1px dashed var(--border);
            padding-bottom: 8px;
            margin-bottom: 8px;
        }
        .brand-info h1 {
            color: var(--primary);
            font-size: 14px;
            font-weight: 800;
            text-transform: uppercase;
            margin-bottom: 2px;
            letter-spacing: -0.3px;
        }
        .brand-info p {
            font-size: 9px;
            color: var(--text-secondary);
            line-height: 1.3;
        }
        .invoice-details {
            margin-top: 4px;
            text-align: center;
        }
        .invoice-details h2 {
            font-size: 13px;
            color: var(--text-main);
            margin-bottom: 2px;
            font-weight: 800;
            letter-spacing: -0.5px;
        }
        .invoice-details p { font-size: 9px; font-weight: 600; }
        .bill-meta {
            display: flex;
            flex-direction: column;
            gap: 4px;
            margin-bottom: 8px;
            border-bottom: 1px dashed var(--border);
            padding-bottom: 6px;
        }
        .bill-to h3 {
            font-size: 9px;
            color: var(--text-secondary);
            margin-bottom: 2px;
            text-transform: uppercase;
            font-weight: 700;
        }
        .bill-to p { font-size: 10px; line-height: 1.4; }
        .bill-summary p { font-size: 10px; line-height: 1.4; text-align: left; }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 8px;
            table-layout: fixed;
        }
        th {
            background-color: var(--primary-light);
            color: var(--primary);
            text-align: left;
            padding: 4px 2px;
            font-size: 9px;
            text-transform: uppercase;
            font-weight: 700;
            border-bottom: 1.5px solid var(--primary);
        }
        td {
            padding: 4px 2px;
            border-bottom: 1px solid var(--border);
            font-size: 9px;
            word-wrap: break-word;
        }
        .totals-container {
            display: flex;
            justify-content: flex-end;
            margin-top: 6px;
        }
        .totals { width: 100%; }
        .total-row {
            display: flex;
            justify-content: space-between;
            padding: 3px 0;
            font-size: 10px;
        }
        .total-row.grand-total {
            border-top: 1.5px solid var(--primary);
            margin-top: 4px;
            padding-top: 4px;
            font-weight: 800;
            font-size: 12px;
            color: var(--primary);
        }
        .doc-footer {
            margin-top: 12px;
            padding-top: 8px;
            border-top: 1px dashed var(--border);
            display: flex;
            flex-direction: column;
            gap: 8px;
            font-size: 8px;
            color: var(--text-secondary);
            text-align: center;
        }
        .signature { text-align: center; width: 100%; margin-top: 6px; }
        .sig-line { border-top: 1px solid var(--text-main); margin-bottom: 4px; width: 100px; margin-left: auto; margin-right: auto; }
    </style>
</head>
<body>
    <div class="document-wrapper">
        <header>
            <div class="brand-info">
                <h1>${d.labName || 'iScan Diagnostics'}</h1>
                <p>${d.labAddress || ''}</p>
                <p>${d.labContact || ''}</p>
            </div>
            <div class="invoice-details">
                <h2>INVOICE</h2>
                <p><strong>No:</strong> ${d.requestsnoprint || ''}</p>
                <p><strong>Date:</strong> ${billDate} ${billTime}</p>
            </div>
        </header>

        <div class="bill-meta">
            <div class="bill-to">
                <h3>Bill To:</h3>
                <p><strong style="font-size: 12px; color: #202124;">${d.name || d.patientname || ''}</strong></p>
                <p>Patient ID: ${d.custid || ''}</p>
                <p>Age/Sex: ${ageStr}</p>
                <p>Address: ${d.address || 'N/A'}</p>
                <p>Mobile: ${d.mobileno || ''}</p>
            </div>
            <div class="bill-summary">
                <p><strong>Type:</strong> ${d.billtype || 'Regular'}</p>
                <p><strong>Status:</strong> <span style="font-weight: 800; color: ${statusColor}">${statusText}</span></p>
                <p><strong>Method:</strong> ${payMethod}</p>
                <p><strong>Ref By:</strong> ${d.doctorname || 'Self'}</p>
                <p><strong>Sample ID:</strong> ${d.requestbarcode || ''}</p>
            </div>
        </div>

        <table>
            <thead>
                <tr>
                    <th style="width: 15px; text-align: center;">#</th>
                    <th>Description</th>
                    <th style="width: 45px; text-align: right;">Rate</th>
                    <th style="width: 35px; text-align: right;">Disc</th>
                    <th style="width: 50px; text-align: right;">Total</th>
                </tr>
            </thead>
            <tbody>
                ${itemsHtml}
            </tbody>
        </table>

        <div class="totals-container">
            <div class="totals">
                <div class="total-row">
                    <span>Gross Amount:</span>
                    <span>${formatCurrency(grossAmount)}</span>
                </div>
                <div class="total-row">
                    <span>Discount:</span>
                    <span style="color: #d93025">-${formatCurrency(discountAmount)}</span>
                </div>
                <div class="total-row grand-total">
                    <span>Grand Total:</span>
                    <span>${formatCurrency(totalAmount)}</span>
                </div>
                <div class="total-row" style="margin-top: 3px; font-weight: 600;">
                    <span>Received:</span>
                    <span>${formatCurrency(paidAmount)}</span>
                </div>
                <div class="total-row" style="color: #d93025; font-weight: 700; border-top: 1px dashed #dadce0; margin-top: 3px; padding-top: 3px; display: ${balanceRowDisplay};">
                    <span>Balance Due:</span>
                    <span>${formatCurrency(balanceAmount)}</span>
                </div>
            </div>
        </div>

        <div style="margin-top: 8px; font-size: 9px; background: #f8f9fa; padding: 4px 6px; border-radius: 4px; text-align: left;">
            <p><strong>Amount in words:</strong> <span style="text-transform: capitalize;">${amountToWords(totalAmount)}</span></p>
        </div>

        <div class="doc-footer">
            <div style="width: 100%;">
                <p><strong>Notes:</strong></p>
                <p style="margin-top: 2px;">1. This is a computer-generated invoice.</p>
                <p>2. Please bring this bill for report collection.</p>
                <p>3. Reports can be viewed online using your Patient ID.</p>
            </div>
            <div class="signature">
                <div class="sig-line"></div>
                <p><strong>Authorized Signatory</strong></p>
                <p style="font-size: 8px; margin-top: 1px;">${d.labName || 'iScan Diagnostics'}</p>
            </div>
        </div>
    </div>
</body>
</html>`;
    }

    // ── Safe protocol launcher ─────────────────────────────────────
    function launchProtocol(url) {
        const a = document.createElement('a');
        a.href = url;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        setTimeout(() => document.body.removeChild(a), 500);
    }

    // ── Load html2pdf.js dynamically ───────────────────────────────
    let html2pdfLoaded = false;
    function ensureHtml2Pdf() {
        return new Promise((resolve, reject) => {
            if (html2pdfLoaded && window.html2pdf) {
                resolve();
                return;
            }
            const script = document.createElement('script');
            script.src = 'https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js';
            script.onload = () => { html2pdfLoaded = true; resolve(); };
            script.onerror = () => reject(new Error('Failed to load html2pdf.js'));
            document.head.appendChild(script);
        });
    }

    function openPdfInNewTab(base64String) {
        try {
            if (base64String.startsWith('data:application/pdf;base64,')) {
                base64String = base64String.substring('data:application/pdf;base64,'.length);
            }
            const byteCharacters = atob(base64String.trim());
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], { type: 'application/pdf' });
            const blobUrl = URL.createObjectURL(blob);
            
            const newWindow = window.open();
            if (newWindow) {
                newWindow.document.write(`
                    <!DOCTYPE html>
                    <html lang="en">
                    <head>
                        <meta charset="UTF-8">
                        <title>Report Preview</title>
                        <style>
                            body, html { margin: 0; padding: 0; width: 100%; height: 100%; overflow: hidden; background-color: #525659; }
                            iframe { width: 100%; height: 100%; border: none; }
                        </style>
                    </head>
                    <body>
                        <iframe src="${blobUrl}#toolbar=0&navpanes=0"></iframe>
                    </body>
                    </html>
                `);
                newWindow.document.close();
            } else {
                alert('Please allow popups for this website to preview reports.');
            }
        } catch (err) {
            console.error('PDF preview failed:', err);
        }
    }

    // ── Generate PDF and preview in browser ──────────────────────────
    window.generateBillPdf = async function (billDataJson) {
        try {
            const billData = typeof billDataJson === 'string' ? JSON.parse(billDataJson) : billDataJson;
            const htmlContent = buildBillHtml(billData);

            await ensureHtml2Pdf();

            // Create a hidden container to render the HTML
            const container = document.createElement('div');
            container.style.position = 'fixed';
            container.style.left = '-9999px';
            container.style.top = '0';
            container.style.width = '80mm';
            container.innerHTML = htmlContent;
            document.body.appendChild(container);

            // Find the document wrapper inside
            const element = container.querySelector('.document-wrapper') || container;

            const opt = {
                margin: 0,
                filename: 'bill.pdf',
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2, useCORS: true, letterRendering: true },
                jsPDF: { unit: 'mm', format: [80, 180], orientation: 'portrait' }
            };

            // Generate PDF as blob
            const pdfBlob = await html2pdf().set(opt).from(element).outputPdf('blob');
            document.body.removeChild(container);

            // Convert blob to base64
            const reader = new FileReader();
            const base64 = await new Promise((resolve, reject) => {
                reader.onloadend = () => {
                    const base64data = reader.result.split(',')[1]; // Remove "data:application/pdf;base64,"
                    resolve(base64data);
                };
                reader.onerror = reject;
                reader.readAsDataURL(pdfBlob);
            });

            openPdfInNewTab(base64);

            return true;
        } catch (err) {
            console.error('Bill PDF generation failed:', err);
            throw err;
        }
    };

    // ── Preview Bill in Modal ──────────────────────────────────────
    window.previewBillHtml = function (billDataJson) {
        try {
            const billData = typeof billDataJson === 'string' ? JSON.parse(billDataJson) : billDataJson;
            const htmlContent = buildBillHtml(billData);

            // Create or reuse the modal
            let modal = document.getElementById('billPreviewModal');
            if (modal) modal.remove();

            modal = document.createElement('div');
            modal.id = 'billPreviewModal';
            modal.style.cssText = 'position:fixed;inset:0;z-index:10000;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.6);backdrop-filter:blur(4px);animation:fadeIn 0.2s ease;';

            const modalContent = document.createElement('div');
            modalContent.style.cssText = 'background:#fff;border-radius:16px;width:90vw;max-width:400px;height:90vh;display:flex;flex-direction:column;box-shadow:0 25px 50px -12px rgba(0,0,0,0.25);overflow:hidden;';

            // Header
            const header = document.createElement('div');
            header.style.cssText = 'display:flex;justify-content:space-between;align-items:center;padding:16px 24px;border-bottom:1px solid #e2e8f0;background:#f8fafc;';
            header.innerHTML = `
                <div style="display:flex;align-items:center;gap:12px;">
                    <span class="material-symbols-outlined" style="color:#4f46e5;font-size:24px;">receipt_long</span>
                    <div>
                        <h3 style="margin:0;font-size:16px;font-weight:700;color:var(--lc-secondary);">Bill Preview</h3>
                        <p style="margin:0;font-size:12px;color:#64748b;">${billData.requestsnoprint || ''} • ${billData.name || billData.patientname || ''}</p>
                    </div>
                </div>
                <button id="closePreviewBtn" style="width:36px;height:36px;border-radius:10px;border:1px solid #e2e8f0;background:#fff;cursor:pointer;display:flex;align-items:center;justify-content:center;transition:all 0.15s;">
                    <span class="material-symbols-outlined" style="font-size:20px;color:#64748b;">close</span>
                </button>`;
            modalContent.appendChild(header);

            // Iframe
            const iframe = document.createElement('iframe');
            iframe.style.cssText = 'flex:1;border:none;width:100%;';
            modalContent.appendChild(iframe);

            modal.appendChild(modalContent);
            document.body.appendChild(modal);

            // Write HTML to iframe
            iframe.contentDocument.open();
            iframe.contentDocument.write(htmlContent);
            iframe.contentDocument.close();

            // Close handlers
            document.getElementById('closePreviewBtn').onclick = () => modal.remove();
            modal.onclick = (e) => { if (e.target === modal) modal.remove(); };
            document.addEventListener('keydown', function escHandler(e) {
                if (e.key === 'Escape') { modal.remove(); document.removeEventListener('keydown', escHandler); }
            });

            return true;
        } catch (err) {
            console.error('Bill preview failed:', err);
            throw err;
        }
    };

    // ── Add fade-in animation style ────────────────────────────────
    const style = document.createElement('style');
    style.textContent = '@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }';
    document.head.appendChild(style);

    // ── Send base64 PDF to external browser window ─────────────────
    window.openBase64Pdf = function (base64String) {
        openPdfInNewTab(base64String);
    };

    window.openPdfPreview = function (base64String) {
        openPdfInNewTab(base64String);
    };

    window.previewBillPdf = async function (billDataJson) {
        try {
            const billData = typeof billDataJson === 'string' ? JSON.parse(billDataJson) : billDataJson;
            const htmlContent = buildBillHtml(billData);

            await ensureHtml2Pdf();

            // Create a hidden container to render the HTML
            const container = document.createElement('div');
            container.style.position = 'fixed';
            container.style.left = '-9999px';
            container.style.top = '0';
            container.style.width = '80mm';
            container.innerHTML = htmlContent;
            document.body.appendChild(container);

            // Find the document wrapper inside
            const element = container.querySelector('.document-wrapper') || container;

            const opt = {
                margin: 0,
                filename: 'bill.pdf',
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2, useCORS: true, letterRendering: true },
                jsPDF: { unit: 'mm', format: [80, 180], orientation: 'portrait' }
            };

            // Generate PDF as blob
            const pdfBlob = await html2pdf().set(opt).from(element).outputPdf('blob');
            document.body.removeChild(container);

            // Convert blob to base64
            const reader = new FileReader();
            const base64 = await new Promise((resolve, reject) => {
                reader.onloadend = () => {
                    const base64data = reader.result.split(',')[1]; // Remove "data:application/pdf;base64,"
                    resolve(base64data);
                };
                reader.onerror = reject;
                reader.readAsDataURL(pdfBlob);
            });

            openPdfInNewTab(base64);

            return true;
        } catch (err) {
            console.error('Bill PDF preview generation failed:', err);
            throw err;
        }
    };

})();
