using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    using System.Collections.Generic;

    public class CreditSafeNonLtdBaseData
    {
        public CreditSafeNonLtdBaseData()
        {
            Tels = new List<CreditSafeNonLtdBaseDataTel>();
            Faxs = new List<CreditSafeNonLtdBaseDataFax>();
            Ratings = new List<CreditSafeNonLtdRatings>();
            Limits = new List<CreditSafeNonLtdLimits>();
            MatchedCcj = new List<CreditSafeNonLtdMatchedCCJ>();
            PossibleCcj = new List<CreditSafeNonLtdPossibleCCJ>();
            Events = new List<CreditSafeNonLtdEvents>();
        }

        [PK(true)]
        [NonTraversable]
        public long CreditSafeNonLtdBaseDataID { get; set; }

        [FK("MP_ServiceLog", "Id")]
        public long? ServiceLogID { get; set; }

        public string EzbobCompanyID { get; set; }
        public bool? HasCreditSafeError { get; set; }
        public bool? HasParsingError { get; set; }
        public string Error { get; set; }
        public DateTime? InsertDate { get; set; }

        [Length(10)]
        public string Number { get; set; }

        [Length(100)]
        public string Name { get; set; }

        [Length(100)]
        public string Address1 { get; set; }

        [Length(100)]
        public string Address2 { get; set; }

        [Length(100)]
        public string Address3 { get; set; }

        [Length(100)]
        public string Address4 { get; set; }

        [Length(10)]
        public string PostCode { get; set; }

        public bool? MpsRegistered { get; set; }
        public DateTime? AddressDate { get; set; }

        [Length(100)]
        public string AddressReason { get; set; }

        [Length(100)]
        public string PremiseType { get; set; }

        [Length(100)]
        public string Activities { get; set; }

        public int? Employees { get; set; }

        [Length(100)]
        public string Website { get; set; }

        [Length(100)]
        public string Email { get; set; }

        public int? MatchedCcjValue { get; set; }
        public int? MatchedCcjNumber { get; set; }
        public DateTime? MatchedCcjDateFrom { get; set; }
        public DateTime? MatchedCcjDateTo { get; set; }
        public int? PossibleCcjValue { get; set; }
        public int? PossibleCcjNumber { get; set; }
        public DateTime? PossibleCcjDateFrom { get; set; }
        public DateTime? PossibleCcjDateTo { get; set; }

        [Length(100)]
        public string ExecutiveName { get; set; }

        [Length(100)]
        public string ExecutivePosition { get; set; }

        [Length(100)]
        public string ExecutiveEmail { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdBaseDataTel> Tels { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdBaseDataFax> Faxs { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdRatings> Ratings { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdLimits> Limits { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdMatchedCCJ> MatchedCcj { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdPossibleCCJ> PossibleCcj { get; set; }

        [NonTraversable]
        public List<CreditSafeNonLtdEvents> Events { get; set; }
    }
}
