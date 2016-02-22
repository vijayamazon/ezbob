namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanAgreements : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanAgreementID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[Length(250)]
		[DataMember]
		public string FilePath { get; set; }

		[FK("LoanAgreementTemplate", "Id")]
		[DataMember]
		public int LoanAgreementTemplateID { get; set; }

		/*/// <summary>
		/// prints data only
		/// to print headers line call base static PrintHeadersLine 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}*/

	} // class NL_LoanAgreements
} // ns
