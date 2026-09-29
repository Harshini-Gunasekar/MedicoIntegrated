using Microsoft.AspNetCore.Http;

namespace medico_backend.InventoryModel
{
    // ─── What the API receives (multipart/form-data) ──────────────────────────
    public class opening_stock_upload_request
    {
        public IFormFile file { get; set; } = null!;

        // Excel has no warehouse column, so the warehouse is sent with the upload
        public long warehousecode { get; set; }

        public string? branchcode { get; set; }
        public long? usercode { get; set; }
        public string? companycode { get; set; }

        // true  = save the valid rows, skip the bad ones and report them
        // false = if ANY row has an error, save nothing (default)
        public bool skiperrors { get; set; } = false;

        // Used when the Excel batch no is blank (71 rows in your file). Send empty to make batch mandatory.
        public string? defaultbatchno { get; set; } = "NA";
    }

    // ─── One parsed row from the Excel sheet ──────────────────────────────────
    public class opening_stock_excel_row
    {
        public int rownumber { get; set; }
        public string itemname { get; set; } = string.Empty;
        public string batchno { get; set; } = string.Empty;
        public DateTime? expirydate { get; set; }
        public decimal openingstrip { get; set; }
        public decimal openingloose { get; set; }
    }

    // ─── Item info needed to calculate opening stock ──────────────────────────
    public class opening_stock_item_info
    {
        public long itemcode { get; set; }
        public string itemname { get; set; } = string.Empty;
        public decimal packsize { get; set; }
        public decimal purchaserate { get; set; }
        public decimal mrp { get; set; }
    }

    // ─── Existing stock row lookup ────────────────────────────────────────────
    public class opening_stock_existing
    {
        public long stockcode { get; set; }
        public long itemcode { get; set; }
        public string? batchno { get; set; }
    }

    // ─── Error reported back per Excel row ────────────────────────────────────
    public class opening_stock_upload_error
    {
        public int rownumber { get; set; }
        public string? itemname { get; set; }
        public string? batchno { get; set; }
        public string message { get; set; } = string.Empty;
        public string? expirydate { get; set; }
        public string? expiry_date { get; set; }
        public decimal? openingstrip { get; set; }
        public decimal? opening_strip { get; set; }
        public decimal? openingloose { get; set; }
        public decimal? opening_loose { get; set; }
        public string? rejectionreason { get; set; }
        public string? rejection_reason { get; set; }
    }

    // ─── Upload result ────────────────────────────────────────────────────────
    public class opening_stock_upload_result
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public int totalrows { get; set; }
        public int inserted { get; set; }
        public int updated { get; set; }
        public int skipped { get; set; }
        public List<opening_stock_upload_error> errors { get; set; } = new();
        public List<string> warnings { get; set; } = new();
    }

    // ─── API Envelope ─────────────────────────────────────────────────────────
    public class opening_stock_api_response
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public opening_stock_upload_result? Data { get; set; }
    }
}