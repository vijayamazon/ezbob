namespace AutomationCalculator.AutoDecision.AutoApproval
{
	using System;

	public class Name: IEquatable<Name> {
		public Name(string first, string last) {
			if (!string.IsNullOrEmpty(first)) {
				first = first.Trim().ToLowerInvariant();
			}

			if (!string.IsNullOrEmpty(last))
			{
				last = last.Trim().ToLowerInvariant();
			}

			FirstName = first;
			LastName = last;
		}
			
		public string FirstName { get; private set; }
		public string LastName { get; private set; }

		public bool Equals(Name other) {
			return FirstName.Equals(other.FirstName) && LastName.Equals(other.LastName);
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", FirstName, LastName);
		}
	}
}
