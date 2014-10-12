namespace EzBob.Backend.Strategies.VatReturn {
	using System.Text;
	using ConfigManager;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class BusinessDataOutput {
			#region properties saved in DB

			public virtual decimal? PctOfAnnualRevenues { get; set; }
			public virtual decimal? Revenues { get; set; }
			public virtual decimal? Opex { get; set; }
			public virtual decimal? TotalValueAdded { get; set; }
			public virtual decimal? PctOfRevenues { get; set; }
			public virtual decimal? Salaries { get; set; }
			public virtual decimal? Tax { get; set; }
			public virtual decimal? Ebida { get; set; }
			public virtual decimal? PctOfAnnual { get; set; }
			public virtual decimal? ActualLoanRepayment { get; set; }
			public virtual decimal? FreeCashFlow { get; set; }

			#region property SalariesMultiplier 

			public virtual decimal SalariesMultiplier {
				get {
					if (m_nSalariesMultiplier == null)
						m_nSalariesMultiplier = (decimal)CurrentValues.Instance.HmrcSalariesMultiplier;

					return m_nSalariesMultiplier.Value;
				} // get

				// ReSharper disable ValueParameterNotUsed
				set {
					// for ITraversable
				} // set
				// ReSharper restore ValueParameterNotUsed
			} // SalariesMultiplier

			private decimal? m_nSalariesMultiplier;

			#endregion property SalariesMultiplier 

			#endregion properties saved in DB

			#region property SalariesCalculated

			public virtual decimal SalariesCalculated {
				get { return (Salaries ?? 0) * SalariesMultiplier; }
			} // SalariesCalculated

			#endregion property SalariesCalculated

			#region method MonthCount

			public virtual int MonthCount() {
				return 1;
			} // MonthCount

			#endregion method MonthCount

			#region method AddSalary

			public virtual void AddSalary(decimal? nDelta) {
				if (nDelta == null)
					return;

				if (!Salaries.HasValue)
					Salaries = 0;

				Salaries += nDelta;
			} // AddSalary

			#endregion method AddSalary

			#region method ToString

			protected virtual void ToString(StringBuilder os, string sPrefix) {
				os.AppendFormat("\n{0}% of annual revenues: {1}", sPrefix, PctOfAnnualRevenues);
				os.AppendFormat("\n{0}Revenues: {1}", sPrefix, Revenues);
				os.AppendFormat("\n{0}Opex: {1}", sPrefix, Opex);
				os.AppendFormat("\n{0}Total value added: {1}", sPrefix, TotalValueAdded);
				os.AppendFormat("\n{0}% of revenues: {1}", sPrefix, PctOfRevenues);
				os.AppendFormat("\n{0}Salaries: {1}", sPrefix, Salaries);
				os.AppendFormat("\n{0}Salaries multiplier: {1}", sPrefix, SalariesMultiplier);
				os.AppendFormat("\n{0}Salaries calculated: {1}", sPrefix, SalariesCalculated);
				os.AppendFormat("\n{0}Tax: {1}", sPrefix, Tax);
				os.AppendFormat("\n{0}Ebida: {1}", sPrefix, Ebida);
				os.AppendFormat("\n{0}% of annual: {1}", sPrefix, PctOfAnnual);
				os.AppendFormat("\n{0}Actual loan repayment: {1}", sPrefix, ActualLoanRepayment);
				os.AppendFormat("\n{0}Free cash flow: {1}", sPrefix, FreeCashFlow);
			} // ToString

			#endregion method ToString
		} // class BusinessDataOutput
	} // class CalculateVatReturnSummary
} // namespace
