namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_OfferFees {
		[PK(true)]
		[DataMember]
		public long OfferFeeID { get; set; }

		[FK("NL_Offers", "OfferID")]
		[DataMember]
		public long OfferID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		public int LoanFeeTypeID { get; set; }

		[DataMember]
		public decimal? Percent { get; set; }

		[DataMember]
		public decimal? AbsoluteAmount { get; set; }

		[DataMember]
		public decimal? OneTimePartPercent { get; set; }

		[DataMember]
		public decimal? DistributedPartPercent { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(GetType().Name + ": ");
			Type t = typeof(NL_OfferFees);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}
	}//class NL_OfferFees
}//ns