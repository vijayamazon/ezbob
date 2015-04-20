using System;
using System.Collections.Generic;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeBaseData
    {

        public CreditSafeBaseData()
        {
            SecondarySicCodes = new List<CreditSafeBaseData_SecondarySicCodes>();
            Industries = new List<CreditSafeIndustries>();
            TradingAddresseses = new List<CreditSafeTradingAddresses>();
            CreditRatings = new List<CreditSafeCreditRatings>();
            CreditLimits = new List<CreditSafeCreditLimits>();
            PreviousNames = new List<CreditSafePreviousNames>();
            CcjDetails = new List<CreditSafeCCJDetails>();
            StatusHistory = new List<CreditSafeStatusHistory>();
            Mortgages = new List<CreditSafeMortgages>();
            ShareHolders = new List<CreditSafeShareHolders>();
            Financial = new List<CreditSafeFinancial>();
            EventHistory = new List<CreditSafeEventHistory>();
            Directors = new List<CreditSafeDirectors>();
        }

        [PK(true)]
        [NonTraversable]
        public long CreditSafeBaseDataID { get; set; }
        [FK("MP_ServiceLog", "Id")]
        public long? ServiceLogID { get; set; }
        [Length(10)]
        public string CompanyRefNum { get; set; }
        public bool? HasCreditSafeError { get; set; }
        public bool? HasParsingError { get; set; }
        [Length("MAX")]
        public string Error { get; set; }
        public DateTime? InsertDate { get; set; }
        [Length(10)]
        public string Number { get; set; }
        [Length(100)]
        public string Name { get; set; }
        [Length(20)]
        public string Telephone { get; set; }
        public bool? TpsRegistered { get; set; }
        [Length(100)]
        public string Address1 { get; set; }
        [Length(100)]
        public string Address2 { get; set; }
        [Length(100)]
        public string Address3 { get; set; }
        [Length(100)]
        public string Address4 { get; set; }
        [Length(10)]
        public string Postcode { get; set; }
        [Length(10)]
        public string SicCode { get; set; }
        [Length(500)]
        public string SicDescription { get; set; }
        [Length(100)]
        public string Website { get; set; }
        [Length(500)]
        public string CompanyType { get; set; }
        [Length(100)]
        public string AccountsType { get; set; }
        public DateTime? AnnualReturnDate { get; set; }
        public DateTime? IncorporationDate { get; set; }
        public DateTime? AccountsFilingDate { get; set; }
        public DateTime? LatestAccountsDate { get; set; }
        [Length(10)]
        public string Quoted { get; set; }
        [Length(10)]
        public string CompanyStatus { get; set; }
        public int? CCJValues { get; set; }
        public int? CCJNumbers { get; set; }
        public DateTime? CCJDateFrom { get; set; }
        public DateTime? CCJDateTo { get; set; }
        public int? CCJNumberOfWrits { get; set; }
        public int? Outstanding { get; set; }
        public int? Satisfied { get; set; }
        public int? ShareCapital { get; set; }

        [NonTraversable]
        public List<CreditSafeBaseData_SecondarySicCodes> SecondarySicCodes { get; set; }
        [NonTraversable]
        public List<CreditSafeIndustries> Industries { get; set; }
        [NonTraversable]
        public List<CreditSafeTradingAddresses> TradingAddresseses { get; set; }
        [NonTraversable]
        public List<CreditSafeCreditRatings> CreditRatings { get; set; }
        [NonTraversable]
        public List<CreditSafeCreditLimits> CreditLimits { get; set; }
        [NonTraversable]
        public List<CreditSafePreviousNames> PreviousNames { get; set; }
        [NonTraversable]
        public List<CreditSafeCCJDetails> CcjDetails { get; set; }
        [NonTraversable]
        public List<CreditSafeStatusHistory> StatusHistory { get; set; }
        [NonTraversable]
        public List<CreditSafeMortgages> Mortgages { get; set; }
        [NonTraversable]
        public List<CreditSafeShareHolders> ShareHolders { get; set; }
        [NonTraversable]
        public List<CreditSafeFinancial> Financial { get; set; }
        [NonTraversable]
        public List<CreditSafeEventHistory> EventHistory { get; set; }
        [NonTraversable]
        public List<CreditSafeDirectors> Directors { get; set; }
    }
}
