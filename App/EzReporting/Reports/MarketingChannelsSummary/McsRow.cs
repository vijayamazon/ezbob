namespace Reports.MarketingChannelsSummary {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using TraficReport;

	internal class ToStringAttribute : Attribute {}

	internal class McsRow {
		[ToString]
		public Source Source { get; set; }
		public int Visitors { get; set; }
		public int StartRegistration { get; set; }
		public int Personal { get; set; }
		public int Company { get; set; }
		public int DataSource { get; set; }
		public decimal RequestedAmount { get; set; }
		public int CompleteApplication { get; set; }
		public int Approved { get; set; }
		public int Rejected { get; set; }
		public decimal ApprovedAmount { get; set; }
		public decimal LoansGiven { get; set; }
		public string Css { get; set; }

		public override string ToString() {
			var os = new List<string>();

			this.Traverse((oInstance, oPropInfo) => os.Add(string.Format("{0}: {1}", oPropInfo.Name, oPropInfo.GetValue(oInstance))));

			return string.Join(", ", os);
		} // ToString

		public void Add(McsRow r) {
			if (r == null)
				return;

			this.Traverse((oInstance, oPropInfo) => {
				if ((oPropInfo.PropertyType != typeof(int)) && (oPropInfo.PropertyType != typeof(decimal)))
					return;

				if (oPropInfo.PropertyType == typeof(int))
					oPropInfo.SetValue(oInstance, (int)oPropInfo.GetValue(oInstance) + (int)oPropInfo.GetValue(r));
				else
					oPropInfo.SetValue(oInstance, (decimal)oPropInfo.GetValue(oInstance) + (decimal)oPropInfo.GetValue(r));
			});
		} // Add
	} // class McsRow
} // namespace
