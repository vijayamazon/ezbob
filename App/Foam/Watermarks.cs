namespace Foam {
	internal class Watermarks {
		public Watermarks(decimal step) {
			this.step = step;
			Approved = step;
			Issued = step;
		} // constructor

		public decimal Approved { get; private set; }
		public decimal Issued { get; private set; }

		public void MoveApproved(decimal actualAmount) {
			while (Approved <= actualAmount)
				Approved += this.step;
		} // MoveApproved

		public void MoveIssued(decimal actualAmount) {
			while (Issued <= actualAmount)
				Issued += this.step;
		} // MoveIssued

		private readonly decimal step;
	} // class Watermarks
} // namespace
