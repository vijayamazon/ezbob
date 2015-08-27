namespace Foam {
	internal class Amounts {
		public Amounts(decimal approved, decimal issued) {
			Approved = approved;
			Issued = issued;
		} // constructor

		public decimal Approved { get; private set; }
		public decimal Issued { get; private set; }

		public override string ToString() {
			return string.Format(
				"approved = {0}, issued = {1}",
				Approved.ToString("C0", Program.Culture),
				Issued.ToString("C0", Program.Culture)
			);
		} // ToString
	} // class Amounts
} // namespace
