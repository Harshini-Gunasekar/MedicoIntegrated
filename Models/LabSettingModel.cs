using System;

namespace Booking.Models
{
    public class LabSettingModel
    {
        public Guid lsid { get; set; } = Guid.NewGuid();

        public bool? ls_common_normal_values { get; set; } = false;
        public bool? ls_select_normal_values { get; set; } = false;
        public bool? ls_hide_signature { get; set; } = false;
        public bool? ls_signature_on_end { get; set; } = false;
        public bool? ls_signature_on_each_page { get; set; } = false;

        public string? lab_report_name { get; set; }
        public string? report_header { get; set; }
        public string? report_footer { get; set; }
        public string? cheque_name { get; set; }

        public bool? timing_normal { get; set; } = false;
        public bool? timing_manual { get; set; } = false;

        public bool? bill_white_sheet { get; set; } = false;
        public bool? bill_letter_pad { get; set; } = false;
        public bool? bill_portrait { get; set; } = true;
        public bool? bill_landscape { get; set; } = false;
        public string? bill_paper_size { get; set; } = "A4";
        public string? bill_orientation { get; set; } = "portrait";

        // Routine Report Signees
        public bool? auth1_show { get; set; } = true;
        public string? auth1_name { get; set; }
        public string? auth1_designation { get; set; }
        public string? auth1_signature_path { get; set; }

        public bool? auth2_show { get; set; } = true;
        public string? auth2_name { get; set; }
        public string? auth2_designation { get; set; }
        public string? auth2_signature_path { get; set; }

        public bool? auth3_show { get; set; } = true;
        public string? auth3_name { get; set; }
        public string? auth3_designation { get; set; }
        public string? auth3_signature_path { get; set; }

        // Culture Report Signees
        public bool? culture_auth1_show { get; set; } = true;
        public string? culture_auth1_name { get; set; }
        public string? culture_auth1_designation { get; set; }
        public string? culture_auth1_signature_path { get; set; }

        public bool? culture_auth2_show { get; set; } = true;
        public string? culture_auth2_name { get; set; }
        public string? culture_auth2_designation { get; set; }
        public string? culture_auth2_signature_path { get; set; }

        public bool? culture_auth3_show { get; set; } = true;
        public string? culture_auth3_name { get; set; }
        public string? culture_auth3_designation { get; set; }
        public string? culture_auth3_signature_path { get; set; }

        // Display Toggles
        public bool? show_bill_header_footer_image { get; set; } = true;
        public bool? show_report_header_footer_image { get; set; } = true;
        public bool? show_culture_header_footer_image { get; set; } = true;
        public bool? show_receipt_header_footer_image { get; set; } = true;
        public bool? show_op_casesheet_header_footer_image { get; set; } = true;
        public bool? show_ip_casesheet_header_footer_image { get; set; } = true;
        public bool? show_casesheet_header_footer_image { get; set; } = true;
        public bool? show_dischargesummary_header_footer_image { get; set; } = true;

        public bool? use_labsetting_signatures { get; set; } = true;
        public bool? use_labsetting_culture_signatures { get; set; } = true;

        public bool? report_qr { get; set; } = true;

        public double? bill_top { get; set; } = 0;
        public double? bill_bottom { get; set; } = 0;

        // iScan Margins
        public double? iscan_margin_top { get; set; } = 0;
        public double? iscan_margin_bottom { get; set; } = 0;
        public double? iscan_margin_left { get; set; } = 0;
        public double? iscan_margin_right { get; set; } = 0;

        // Culture Margins
        public double? culture_margin_top { get; set; } = 0;
        public double? culture_margin_bottom { get; set; } = 0;
        public double? culture_margin_left { get; set; } = 0;
        public double? culture_margin_right { get; set; } = 0;

        public bool? print_work_order { get; set; } = false;
        public bool? print_online_code_in_bill { get; set; } = false;
        public bool? cust_name_upper { get; set; } = false;
        public bool? direct_result { get; set; } = false;
        public bool? ignore_bill_print { get; set; } = false;

        public bool? sig_name_type { get; set; } = false;
        public bool? jp_normal_alert { get; set; } = false;
        public bool? maintain_patient { get; set; } = false;

        public bool? multi_branch { get; set; } = false;
        public bool? authorize_results { get; set; } = false;

        public string? backup_path { get; set; }
        public bool? post_cash_advice { get; set; } = false;

        public int? home_collection_tcode { get; set; }

        public bool? ls_bill_user_defined { get; set; } = false;
        public bool? ls_slip_user_defined { get; set; } = false;

        public bool? ls_culture_normal { get; set; } = false;
        public bool? ls_culture_isolated { get; set; } = false;

        public bool? print_branch_name_in_bill { get; set; } = false;
        public bool? focus_address { get; set; } = false;
        public bool? show_hospital_id { get; set; } = false;

        public bool? pathology_no { get; set; } = false;
        public bool? display_user_name { get; set; } = false;

        public bool? same_day { get; set; } = false;
        public bool? next_day { get; set; } = false;

        public double? regular_discount { get; set; } = 0;

        public bool? ls_collectedby { get; set; } = false;
        public bool? print_bill_to_printer { get; set; } = false;
        public bool? print_barcode { get; set; } = false;

        public bool? ls_send_lab_sms { get; set; } = false;
        public bool? ls_send_scan_sms { get; set; } = false;
        public bool? ls_cancelled_bills { get; set; } = false;
        public bool? ls_confirm_counter { get; set; } = false;

        public bool? sample_collection { get; set; } = false;
        public bool? branch_wise_sample_collection { get; set; } = false;
        public bool? dept_wise_sample_collection { get; set; } = false;
        public bool? print_barcode_directly { get; set; } = false;

        public string? logo_path { get; set; }
        public string? header_path { get; set; }
        public string? footer_path { get; set; }

        public string? tenant_code { get; set; }
        public bool deleted { get; set; } = false;
        public int? bh_code { get; set; }
        public bool? counterset_setting { get; set; } = false;
        public bool? ref_by { get; set; } = false;
        public bool? is_slot_required { get; set; } = true;   // false = walk-in registration without picking a slot
        public bool? op_age_wise_split { get; set; } = false; // true = doctor's OP charge is split by age slab
        public bool? show_all_customers { get; set; } = true; // true = show all customers across tenants, false = show current tenant customers only
    }
}
