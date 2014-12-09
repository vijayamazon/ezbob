namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using System.Text;

	[DataContract(IsReference = true)]
	public class VatReturnSummaryBase { 
		[DataMember]
		public int SummaryID { get; set; }

		[DataMember]
		public virtual decimal? PctOfAnnualRevenues { get; set; }

		[DataMember]
		public virtual decimal? Revenues { get; set; }

		[DataMember]
		public virtual decimal? Opex { get; set; }

		[DataMember]
		public virtual decimal? TotalValueAdded { get; set; }

		[DataMember]
		public virtual decimal? PctOfRevenues { get; set; }

		[DataMember]
		public virtual decimal? Salaries { get; set; }

		[DataMember]
		public virtual decimal? Tax { get; set; }

		[DataMember]
		public virtual decimal? Ebida { get; set; }

		[DataMember]
		public virtual decimal? PctOfAnnual { get; set; }

		[DataMember]
		public virtual decimal? ActualLoanRepayment { get; set; }

		[DataMember]
		public virtual decimal? FreeCashFlow { get; set; }

		protected virtual void ToString(StringBuilder os, string sPrefix) {
			os.AppendFormat("\n{0}% of annual revenues: {1}", sPrefix, PctOfAnnualRevenues);
			os.AppendFormat("\n{0}Revenues: {1}", sPrefix, Revenues);
			os.AppendFormat("\n{0}Opex: {1}", sPrefix, Opex);
			os.AppendFormat("\n{0}Total value added: {1}", sPrefix, TotalValueAdded);
			os.AppendFormat("\n{0}% of revenues: {1}", sPrefix, PctOfRevenues);
			os.AppendFormat("\n{0}Salaries: {1}", sPrefix, Salaries);
			os.AppendFormat("\n{0}Tax: {1}", sPrefix, Tax);
			os.AppendFormat("\n{0}Ebida: {1}", sPrefix, Ebida);
			os.AppendFormat("\n{0}% of annual: {1}", sPrefix, PctOfAnnual);
			os.AppendFormat("\n{0}Actual loan repayment: {1}", sPrefix, ActualLoanRepayment);
			os.AppendFormat("\n{0}Free cash flow: {1}", sPrefix, FreeCashFlow);
		} // ToString

	} // class VatReturnSummaryBase
} // namespace Ezbob.Backend.Models
