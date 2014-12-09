namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using Utils;

	[DataContract(IsReference = true)]
	public class VatReturnSummary : VatReturnSummaryBase {
		public VatReturnSummary() {
			Quarters = new List<VatReturnQuarter>();
		} // VatReturnSummary

		[DataMember]
		public int BusinessID { get; set; }

		[DataMember]
		public long? RegistrationNo { get; set; }

		[DataMember]
		public string BusinessName { get; set; }

		[DataMember]
		public string BusinessAddress { get; set; }

		[DataMember]
		public string CurrencyCode { get; set; }

		[DataMember]
		public virtual decimal? SalariesMultiplier { get; set; }

		[DataMember]
		public decimal? AnnualizedTurnover { get; set; }

		[DataMember]
		public decimal? AnnualizedValueAdded { get; set; }

		[DataMember]
		public decimal? AnnualizedFreeCashFlow { get; set; }

		[DataMember]
		[NonTraversable]
		public List<VatReturnQuarter> Quarters { get; set; }

		public override string ToString() {
			var os = new StringBuilder();

			string sBusiness = string.Format(
				"{0} - {1}: {2}\n{3}",
				BusinessID,
				RegistrationNo,
				BusinessName,
				BusinessAddress
			);

			os.AppendFormat(
				"\n\n\tBusiness: {0}\n\tCurrency code: {1}\n\tSalaries multiplier: {2}\n\n\t*** Quarters:\n\t{3}\n",
				sBusiness,
				CurrencyCode,
				SalariesMultiplier,
				string.Join("\n\t", Quarters.Select(x => x.ToString("\t\t")))
			);

			ToString(os, "\t");

			return os.ToString();
		} // ToString

	} // class VatReturnSummary
} // namespace Ezbob.Backend.Models
