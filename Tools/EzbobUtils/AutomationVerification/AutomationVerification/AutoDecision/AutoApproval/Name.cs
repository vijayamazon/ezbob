namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;

	public class Name : IEquatable<Name> {
		public Name(string first, string last) {
			FirstName = string.IsNullOrWhiteSpace(first) ? string.Empty : first.Trim().ToLowerInvariant();
			LastName  = string.IsNullOrWhiteSpace(last)  ? string.Empty : last.Trim().ToLowerInvariant();
		} // constructor

		public string FirstName { get; private set; }
		public string LastName { get; private set; }

		public bool IsEmpty {
			get { return FirstName == string.Empty && LastName == string.Empty; }
		} // IsEmpty

		public bool Equals(Name other) {
			return FirstName.Equals(other.FirstName) && LastName.Equals(other.LastName);
		} // Equals

		public override string ToString() {
			return string.Format("{0} {1}", FirstName, LastName);
		} // ToString
	} // class Name
} // namespace
