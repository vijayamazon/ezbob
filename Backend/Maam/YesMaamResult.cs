namespace Ezbob.Maam {
	class YesMaamResult {
		public class OneResult {
			public string Data { get; set; }
			public decimal Amount { get; set; }

			public OneResult() {
				Data = "not run";
				Amount = 0;
			} // constructor
		} // OneResult

		public YesMaamInputRow Input { get; private set; }

		public OneResult AutoReject { get; set; }
		public OneResult AutoApprove { get; set; }

		public YesMaamResult(YesMaamInputRow row) {
			Input = row;
			AutoApprove = new OneResult();
			AutoReject = new OneResult();
		} // constructor
	} // class YesMaamResult
} // namespace
