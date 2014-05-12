namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using Utils;

	[DataContract]
	public class VatReturnSummary : VatReturnSummaryBase {
		public VatReturnSummary() {
			Quarters = new List<VatReturnQuarter>();
		} // VatReturnSummary

		[DataMember]
		public int BusinessID { get; set; }

		[DataMember]
		public string CurrencyCode { get; set; }

		[DataMember]
		public virtual decimal? SalariesMultiplier { get; set; }

		[DataMember]
		[NonTraversable]
		public List<VatReturnQuarter> Quarters { get; set; }

		#region method ToString

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat(
				"\n\n\tBusiness ID: {0}\n\tCurrency code: {1}\n\tSalaries multiplier: {2}\n\n\t*** Quarters:\n\t{3}\n",
				BusinessID,
				CurrencyCode,
				SalariesMultiplier,
				string.Join("\n\t", Quarters.Select(x => x.ToString("\t\t")))
			);

			ToString(os, "\t");

			return os.ToString();
		} // ToString

		#endregion method ToString
	} // class VatReturnSummary
} // namespace Ezbob.Backend.Models
