using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace LIMS_Backend.Model
{
    // ═══════════════════════════════════════════════════════════
    // ACTION TYPE CONSTANTS  — used in action_type column
    // ═══════════════════════════════════════════════════════════
    public static class SampleLogAction
    {
        public const string Collected = "COLLECTED";
        public const string Accepted = "ACCEPTED";
        public const string Rejected = "REJECTED";
        public const string Resampling = "RESAMPLING";
        public const string Received = "RECEIVED";
    }

    // ═══════════════════════════════════════════════════════════
    // lab_request_specimencollection
    //   rejectreason / resamplingreason marked [Write(false)]
    //   → Dapper skips them on INSERT/UPDATE
    //   → API still receives them; class routes them to samplelog
    // ═══════════════════════════════════════════════════════════
    [Table("lab_request_specimencollection")]
    public class LabSampleCollectionModel
    {
        [ExplicitKey]
        public Guid lrspid { get; set; }

        public Guid? requestguid { get; set; }
        public string? samplereference { get; set; }
        public int? sno { get; set; }
        public int? defaults_code { get; set; }
        public int? scode { get; set; }
        public DateTime? billedtime { get; set; }
        public DateTime? collectedtime { get; set; }
        public bool? collectedstatus { get; set; }
        public bool? changedmanually { get; set; }
        public int? ctcode { get; set; }
        public string? barcode { get; set; }

        public bool? isaccept { get; set; }
        public DateTime? acceptdatetime { get; set; }

        public bool? isreject { get; set; }
        public DateTime? rejectdatetime { get; set; }

        public bool? is_resampling { get; set; }
        public DateTime? resamplingdatetime { get; set; }

        public bool? isdeleted { get; set; }
        public DateTime? deletedtime { get; set; }
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // lab_request_samplelog  — full audit trail
    //   lrspid (FK) and action_type added for query support.
    //   enteredtime is used as the log creation timestamp.
    // ═══════════════════════════════════════════════════════════
    [Table("lab_request_samplelog")]
    public class LabSampleLogModel
    {
        [ExplicitKey]
        public Guid lrslid { get; set; }

        // ── FIX: was missing; FK to specimencollection ────────────
        public Guid lrspid { get; set; }

        public Guid? requestguid { get; set; }
        public string? samplereference { get; set; }
        public int? scode { get; set; }
        public int? gcode { get; set; }

        // ── FIX: was missing; written by DeriveActionType ─────────
        public string? action_type { get; set; }

        public DateTime? billedtime { get; set; }
        public bool? collectedstatus { get; set; }
        public DateTime? collectedtime { get; set; }
        public bool? receivedstatus { get; set; }
        public DateTime? receivedtime { get; set; }
        public bool? enteredstatus { get; set; }
        public DateTime? enteredtime { get; set; }
        public string? barcode { get; set; }

        public bool? isaccept { get; set; }
        public DateTime? acceptdatetime { get; set; }

        public bool? isreject { get; set; }
        public string? rejectreason { get; set; }
        public DateTime? rejectdatetime { get; set; }

        public bool? is_resampling { get; set; }
        public string? resamplingreason { get; set; }
        public DateTime? resamplingdatetime { get; set; }

        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // lab_request_specimenreceive  — seeded on isaccept = true
    //   BUG FIX: added lrspid FK which was missing but used
    //   in InsertReceiveRowIfMissing and the receive JOIN query
    // ═══════════════════════════════════════════════════════════
    [Table("lab_request_specimenreceive")]
    public class LabSpecimenReceiveModel
    {
        [ExplicitKey]
        public Guid lrsprid { get; set; }

        // ── FIX: was missing; FK to specimencollection ────────────
        public Guid lrspid { get; set; }

        public Guid? requestguid { get; set; }
        public string? samplereference { get; set; }
        public int? defaults_code { get; set; }
        public int? scode { get; set; }
        public DateTime? billedtime { get; set; }
        public DateTime? collectedtime { get; set; }
        public bool? collectedstatus { get; set; }
        public bool? changedmanually { get; set; }
        public int? gcode { get; set; }
        public bool? receivedstatus { get; set; }
        public DateTime? receivedtime { get; set; }
        public bool? alldepartment { get; set; }
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // REQUEST DTO — SaveSampleCollection
    //   Wraps the collection model + reason strings.
    //   Reasons are NOT stored in specimencollection table —
    //   they are routed to samplelog only.
    //   tenant_code is injected by the controller from the header.
    // ═══════════════════════════════════════════════════════════
    public class SaveSampleCollectionRequest
    {
        public LabSampleCollectionModel collection { get; set; } = new();

        /// <summary>Written to samplelog only — never to specimencollection.</summary>
        public string? rejectreason { get; set; }

        /// <summary>Written to samplelog only — never to specimencollection.</summary>
        public string? resamplingreason { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // REQUEST DTO — SaveSampleReceive
    //   lrsprid    comes from LoadSampleReceivedDto.LrspRid
    //   tenant_code is injected by the controller from the header
    // ═══════════════════════════════════════════════════════════
    public class SaveSampleReceiveRequest
    {
        /// <summary>PK of the specimenreceive row to mark as received.</summary>
        public Guid lrsprid { get; set; }

        /// <summary>Injected by controller — do not send in body.</summary>
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // RESPONSE DTO — Get (specific) with logs
    //   Reasons are readable from the Logs list
    // ═══════════════════════════════════════════════════════════
    public class SpecimenWithLogDto
    {
        public LabSampleCollectionModel Collection { get; set; } = new();
        public IList<LabSampleLogModel> Logs { get; set; } = new List<LabSampleLogModel>();
    }

    // ═══════════════════════════════════════════════════════════
    // RESPONSE DTO — loadsamplecollection
    //   Reasons pulled from latest samplelog entry via LEFT JOIN
    // ═══════════════════════════════════════════════════════════
    public class LoadSampleCollectionDto
    {
        public Guid? RequestGuid { get; set; }
        public string? RequestSnoprint { get; set; }
        public string? PatientName { get; set; }
        public string? Gender { get; set; }
        public string? AgeYears { get; set; }
        public DateTimeOffset? RequestDateTime { get; set; }

        public long? Scode { get; set; }
        public string? SampleName { get; set; }
        public string? SampleShortname { get; set; }

        // Collection status
        public Guid? LrspId { get; set; }
        public string? Barcode { get; set; }
        public bool? CollectedStatus { get; set; }
        public DateTime? CollectedTime { get; set; }
        public bool? IsAccept { get; set; }
        public bool? IsReject { get; set; }
        public bool? IsResampling { get; set; }

        // Reasons from samplelog (latest entry)
        public string? RejectReason { get; set; }
        public string? ResamplingReason { get; set; }

        // Received fields (from loadsamplereceived)
        public Guid? LrsprId { get; set; }
        public bool? ReceivedStatus { get; set; }
        public DateTime? ReceivedTime { get; set; }
        public int? Gcode { get; set; }
        public string? GroupName { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    // RESPONSE DTO — loadsamplereceived
    // ═══════════════════════════════════════════════════════════
    public class LoadSampleReceivedDto
    {
        public Guid? LrspId { get; set; }
        public Guid? RequestGuid { get; set; }
        public string? SampleReference { get; set; }
        public string? Barcode { get; set; }
        public DateTime? CollectedTime { get; set; }
        public bool? IsAccept { get; set; }
        public DateTime? AcceptDateTime { get; set; }
        public bool? IsReject { get; set; }

        // Reason from samplelog
        public string? RejectReason { get; set; }

        public string? RequestSnoprint { get; set; }
        public string? PatientName { get; set; }
        public string? Gender { get; set; }
        public string? AgeYears { get; set; }
        public DateTimeOffset? RequestDateTime { get; set; }

        public long? Scode { get; set; }
        public string? SampleName { get; set; }
        public string? SampleShortname { get; set; }

        public Guid? LrsprId { get; set; }
        public bool? ReceivedStatus { get; set; }
        public DateTime? ReceivedTime { get; set; }
        public int? Gcode { get; set; }
        public string? GroupName { get; set; }
    }
}
