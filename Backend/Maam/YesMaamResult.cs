namespace Ezbob.Maam {
	class YesMaamResult {
		#region class OneResult

		public class OneResult {
			public string SameData { get; set; }
			public decimal SameAmount { get; set; }

			public string CurrentData { get; set; }
			public decimal CurrentAmount { get; set; }

			public OneResult() {
				SameData = "not run";
				SameAmount = 0;

				CurrentData = "not run";
				CurrentAmount = 0;
			} // constructor
		} // OneResult

		#endregion class OneResult

		public YesMaamInputRow Input { get; private set; }

		public OneResult AutoReject { get; set; }
		public OneResult AutoApprove { get; set; }

		#region constructor

		public YesMaamResult(YesMaamInputRow oRow) {
			Input = oRow;
			AutoApprove = new OneResult();
			AutoReject = new OneResult();
		} // constructor

		#endregion constructor
	} // class YesMaamResult
} // namespace
