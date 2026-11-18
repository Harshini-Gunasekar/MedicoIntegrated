using System.Text.Json.Serialization;

namespace LabCare.Models
{
    public class DoctorModel
    {
        [JsonPropertyName("dcode")]
        public int dcode { get; set; }

        [JsonPropertyName("orderno")]
        public int? orderno { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("qualification")]
        public int? qualification { get; set; }

        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("spcode")]
        public int? spcode { get; set; }

        [JsonPropertyName("tcode")]
        public int? tcode { get; set; }

        [JsonPropertyName("ftcode")]
        public int? ftcode { get; set; }

        [JsonPropertyName("ricode")]
        public int? ricode { get; set; }

        [JsonPropertyName("procode")]
        public int? procode { get; set; }

        [JsonPropertyName("reference")]
        public string? reference { get; set; }

        [JsonPropertyName("address")]
        public string? address { get; set; }

        [JsonPropertyName("area")]
        public string? area { get; set; }

        [JsonPropertyName("city")]
        public string? city { get; set; }

        [JsonPropertyName("zipcode")]
        public string? zipcode { get; set; }

        [JsonPropertyName("state")]
        public string? state { get; set; }

        [JsonPropertyName("country")]
        public string? country { get; set; }

        [JsonPropertyName("phonestdcode")]
        public string? phonestdcode { get; set; }

        [JsonPropertyName("phone")]
        public string? phone { get; set; }

        [JsonPropertyName("faxstdcode")]
        public string? faxstdcode { get; set; }

        [JsonPropertyName("fax")]
        public string? fax { get; set; }

        [JsonPropertyName("mobile")]
        public string? mobile { get; set; }

        [JsonPropertyName("email")]
        public string? email { get; set; }

        [JsonPropertyName("website")]
        public string? website { get; set; }

        [JsonPropertyName("appdoctor")]
        public bool appdoctor { get; set; }

        [JsonPropertyName("areacode")]
        public int? areacode { get; set; }

        [JsonPropertyName("statecode")]
        public int? statecode { get; set; }

        [JsonPropertyName("countrycode")]
        public int? countrycode { get; set; }

        [JsonPropertyName("importreference")]
        public string? importreference { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; } = false;

        [JsonPropertyName("usercode")]
        public int? usercode { get; set; } = 1;

        [JsonPropertyName("computercode")]
        public int? computercode { get; set; } = 1;

        [JsonPropertyName("entereddate")]
        public DateTime entereddate { get; set; } = DateTime.Now;

        [JsonPropertyName("ibsdate")]
        public DateTime ibsdate { get; set; } = DateTime.Now;

        [JsonPropertyName("initial")]
        public string? initial { get; set; }

        [JsonPropertyName("nametitle")]
        public string? nametitle { get; set; }

        [JsonPropertyName("doctorcode")]
        public string? doctorcode { get; set; }

        [JsonPropertyName("onlinepassword")]
        public string? onlinepassword { get; set; }

        [JsonPropertyName("entrytype")]
        public string? entrytype { get; set; }

        [JsonPropertyName("sendaccounts")]
        public bool sendaccounts { get; set; }

        [JsonPropertyName("sendresults")]
        public bool sendresults { get; set; }

        [JsonPropertyName("sendoffer")]
        public bool sendoffer { get; set; }

        [JsonPropertyName("sendbillinfo")]
        public bool sendbillinfo { get; set; }

        [JsonPropertyName("accountsmobileno")]
        public string? accountsmobileno { get; set; }

        [JsonPropertyName("resultmobileno")]
        public string? resultmobileno { get; set; }

        [JsonPropertyName("contactperson")]
        public string? contactperson { get; set; }

        [JsonPropertyName("doctorimage")]
        public string? doctorimage { get; set; }

        [JsonPropertyName("smslabname")]
        public string? smslabname { get; set; }

        [JsonPropertyName("bedno")]
        public string? bedno { get; set; }

        [JsonPropertyName("doctorfullname")]
        public string? doctorfullname { get; set; }

        [JsonPropertyName("isvariableprice")]
        public bool isvariableprice { get; set; }

        [JsonPropertyName("isic")]
        public bool isic { get; set; }

        [JsonPropertyName("icpaymode")]
        public string? icpaymode { get; set; }

        [JsonPropertyName("icchequename")]
        public string? icchequename { get; set; }

        [JsonPropertyName("creditlimit")]
        public double? creditlimit { get; set; }

        [JsonPropertyName("isseparatespecialrate")]
        public bool isseparatespecialrate { get; set; }

        [JsonPropertyName("separateletterhead")]
        public bool separateletterhead { get; set; }

        [JsonPropertyName("isspecialrate")]
        public bool isspecialrate { get; set; }

        [JsonPropertyName("schemeftcode")]
        public int? schemeftcode { get; set; }

        [JsonPropertyName("specialftcode")]
        public int? specialftcode { get; set; }

        [JsonPropertyName("creditdays")]
        public int? creditdays { get; set; }

        [JsonPropertyName("iscash")]
        public bool iscash { get; set; }

        [JsonPropertyName("iscredit")]
        public bool iscredit { get; set; }

        [JsonPropertyName("icode")]
        public int? icode { get; set; }

        [JsonPropertyName("qualification_dr")]
        public string? qualification_dr { get; set; }

        [JsonPropertyName("bhcode")]
        public int? bhcode { get; set; }

        [JsonPropertyName("rotcode")]
        public int? rotcode { get; set; }

        [JsonPropertyName("skycode")]
        public string? skycode { get; set; }

        [JsonPropertyName("isconsultant")]
        public bool? isconsultant { get; set; }

        [JsonPropertyName("tenant_code")]
        public string? tenant_code { get; set; }
    }
}
