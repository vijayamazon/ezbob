using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {

	public class CallCreditDataApplicant {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditSearchData", "Id")]
		public long SearchDataId { get; set; }
		[FK("CallCreditData", "Id")]
		public int DataId { get; set; }
		public DateTime Dob { get; set; }
		public bool Hho { get; set; }
		public bool TpOptOut { get; set; }
		public int OiaID { get; set; }
		[Length(10)]
		public string CustomerStatus { get; set; }
		[Length(10)]
		public string MaritalStatus { get; set; }
		public int TotalDependents { get; set; }
		[Length(10)]
		public string LanguageVerbal { get; set; }
		[Length(10)]
		public string Type1 { get; set; }
		[Length(10)]
		public string Type2 { get; set; }
		[Length(10)]
		public string Type3 { get; set; }
		[Length(10)]
		public string Type4 { get; set; }
		[Length(10)]
		public string AccommodationType { get; set; }
		public int PropertyValue { get; set; }
		public int MortgageBalance { get; set; }
		public int MonthlyRental { get; set; }
		[Length(10)]
		public string ResidentialStatus { get; set; }
		[Length(10)]
		public string EmailType { get; set; }
		[Length(100)]
		public string EmailAddress { get; set; }
		[Length(10)]
		public string TelephoneType { get; set; }
		[Length(5)]
		public string STD { get; set; }
		[Length(11)]
		public string Number { get; set; }
		[Length(5)]
		public string Extension { get; set; }
		[Length(10)]
		public string Occupation { get; set; }
		[Length(10)]
		public string EmploymentStatus { get; set; }
		public DateTime ExpiryDate { get; set; }
		[Length(10)]
		public string EmploymentRecency { get; set; }
		[Length(10)]
		public string EmployerCategory { get; set; }
		[Length(15)]
		public string TimeAtCurrentEmployer { get; set; }
		[Length(6)]
		public string SortCode { get; set; }
		[Length(20)]
		public string AccountNumber { get; set; }
		[Length(15)]
		public string TimeAtBank { get; set; }
		[Length(10)]
		public string PaymentMethod { get; set; }
		[Length(10)]
		public string FinanceType { get; set; }
		public int TotalDebitCards { get; set; }
		public int TotalCreditCards { get; set; }
		public int MonthlyUnsecuredAmount { get; set; }
		public int AmountPr { get; set; }
		[Length(10)]
		public string TypePr { get; set; }
		[Length(10)]
		public string PaymentMethodPr { get; set; }
		[Length(10)]
		public string FrequencyPr { get; set; }
		public int AmountAd { get; set; }
		[Length(10)]
		public string TypeAd { get; set; }
		[Length(10)]
		public string PaymentMethodAd { get; set; }
		[Length(10)]
		public string FrequencyAd { get; set; }

	}
}
