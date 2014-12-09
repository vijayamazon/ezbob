namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;
	using System.Text;

	[Serializable]
	[DataContract(IsReference = true)]
	public class AffordabilityData : IComparable<AffordabilityData> {
		[DataMember]
		public AffordabilityType Type { get; set; }

		public string TypeStr { get { return Type.ToString(); } }

		[DataMember]
		public DateTime? DateFrom { get; set; }

		[DataMember]
		public DateTime? DateTo { get; set; }

		[DataMember]
		public decimal? Revenues { get; set; }

		[DataMember]
		public decimal? Opex { get; set; }

		[DataMember]
		public decimal? ValueAdded { get; set; }

		[DataMember]
		public decimal? Salaries { get; set; }

		[DataMember]
		public decimal? Tax { get; set; }

		[DataMember]
		public decimal? Ebitda { get; set; }

		[DataMember]
		public decimal? LoanRepayment { get; set; }

		[DataMember]
		public decimal? FreeCashFlow { get; set; }

		[DataMember]
		public string ErrorMsgs { get; set; }

		[DataMember]
		public bool IsAnnualized { get; set; }

		public override string ToString() {
			var os = new StringBuilder();

			var ci = new CultureInfo("en-GB", false);

			os.AppendFormat("Type: {0}", Type);
			os.AppendFormat(" From: {0}", DateFrom.HasValue ? DateFrom.Value.ToString("MMMM d yyyy", ci) : "-- null --");
			os.AppendFormat(" To: {0}", DateTo.HasValue ? DateTo.Value.ToString("MMMM d yyyy", ci) : "-- null --");
			os.AppendFormat(" Revenues: {0}", Revenues.HasValue ? Revenues.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" OPEX: {0}", Opex.HasValue ? Opex.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Value added: {0}", ValueAdded.HasValue ? ValueAdded.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Salaries: {0}", Salaries.HasValue ? Salaries.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Tax: {0}", Tax.HasValue ? Tax.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" EBITDA: {0}", Ebitda.HasValue ? Ebitda.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Loan repayment: {0}", LoanRepayment.HasValue ? LoanRepayment.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Free cash flow: {0}", FreeCashFlow.HasValue ? FreeCashFlow.Value.ToString("C2", ci) : "-- null --");
			os.AppendFormat(" Error msgs: '{0}'", ErrorMsgs);
			os.AppendFormat(" Annualized: {0}", IsAnnualized ? "yes" : "no");

			return os.ToString();
		} // ToString

		public void Fill() {
			Revenues = Revenues ?? 0;
			Opex = Opex ?? 0;
			Salaries = Salaries ?? 0;

			ValueAdded = Revenues - Opex;

			Tax = null; // TODO: some day...

			Ebitda = ValueAdded - Salaries;

			LoanRepayment = 0;

			FreeCashFlow = Ebitda - LoanRepayment;
		} // Fill

		// TODO: annualized trend

		// TODO: quarter trend

		public int CompareTo(AffordabilityData other) {
			return Type.CompareTo(other.Type);
		} // CompareTo

	} // class AffordabilityData
} // namespace
