namespace Reports.MarketingChannelsSummary {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using TraficReport;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal class ToStringAttribute : Attribute {}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal class CumulativeAttribute : Attribute {}

	internal class McsRow {
		[ToString]
		public Source Source { get; set; }

		[Cumulative]
		public int Visitors { get; set; }

		[Cumulative]
		public int StartRegistration { get; set; }

		public decimal WpConversionPct { get; set; }

		[Cumulative]
		public int Personal { get; set; }

		[Cumulative]
		public int Company { get; set; }

		[Cumulative]
		public int DataSource { get; set; }

		[Cumulative]
		public decimal RequestedAmount { get; set; }

		[Cumulative]
		public int CompleteApplication { get; set; }

		public decimal AppComPct { get; set; }

		[Cumulative]
		public int Approved { get; set; }

		[Cumulative]
		public int Rejected { get; set; }

		[Cumulative]
		public int Pending { get; set; }

		[Cumulative]
		public int ApprovedDidntTake { get; set; }

		[Cumulative]
		public decimal ApprovedAmount { get; set; }

		public decimal ApprovedLoanTakenPct { get; set; }

		[Cumulative]
		public decimal LoansGiven { get; set; }

		public decimal ApprovedAvg { get; set; }

		public string Css { get; set; }

		public override string ToString() {
			var os = new List<string>();

			this.Traverse(
				(oInstance, oPropInfo) => os.Add(
					string.Format("{0}: {1}", oPropInfo.Name, oPropInfo.GetValue(oInstance))
				)
			);

			return string.Join(", ", os);
		} // ToString

		public void Add(McsRow r) {
			if (r == null)
				return;

			this.Traverse((oInstance, oPropInfo) => {
				object[] oAttrList = oPropInfo.GetCustomAttributes(typeof(CumulativeAttribute), false);

				if (oAttrList.Length == 0)
					return;

				if (oPropInfo.PropertyType == typeof(int))
					oPropInfo.SetValue(oInstance, (int)oPropInfo.GetValue(oInstance) + (int)oPropInfo.GetValue(r));
				else
					oPropInfo.SetValue(oInstance, (decimal)oPropInfo.GetValue(oInstance) + (decimal)oPropInfo.GetValue(r));
			});
		} // Add

		public void DoMath() {
			WpConversionPct = Ratio(StartRegistration, Visitors);
			AppComPct = Ratio(CompleteApplication, StartRegistration);
			ApprovedLoanTakenPct = Ratio(LoansGiven, ApprovedAmount);
			ApprovedAvg = Ratio(ApprovedAmount, Approved);
		} // DoMath

		private static decimal Ratio(decimal a, decimal b) {
			return Math.Abs(b) < 0.00000001m ? 0 : a / b;
		} // Ratio
	} // class McsRow
} // namespace
