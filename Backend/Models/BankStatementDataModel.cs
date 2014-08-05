namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	[Serializable]
	public class BankStatementDataModel {
		[DataMember]
		public int PeriodMonthsNum { get; set; }
		[DataMember]
		public string Period { get; set; }
		[DataMember]
		public double PercentOfAnnual { get; set; }
		[DataMember]
		public DateTime? DateFrom { get; set; }
		[DataMember]
		public DateTime? DateTo { get; set; }
		[DataMember]
		public double Revenues { get; set; }
		[DataMember]
		public double Opex { get; set; }
		[DataMember]
		public double TotalValueAdded { get; set; }
		[DataMember]
		public double PercentOfRevenues { get; set; }
		[DataMember]
		public double Salaries { get; set; }
		[DataMember]
		public double Tax { get; set; }
		[DataMember]
		public double Ebida { get; set; }
		[DataMember]
		public double PercentOfAnnual2 { get; set; }
		[DataMember]
		public double ActualLoansRepayment { get; set; }
		[DataMember]
		public double FreeCashFlow { get; set; }
	} // class BankStatementDataModel
} // namespace
