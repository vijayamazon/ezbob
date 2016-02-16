namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	/// <summary>
	/// Holds director data.
	/// </summary>
	public class DirectorData {
		/// <summary>
		/// Director first name.
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Director last name.
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Director birth date.
		/// Should be sent to LG as string in format yyyy-mm-dd.
		/// </summary>
		public DateTime DateOfBirth { get; set; }

		/// <summary>
		/// Director address postcode.
		/// </summary>
		public string Postcode {
			get {
				if (string.IsNullOrWhiteSpace(this.postcode))
					this.postcode = string.Empty;

				return this.postcode;
			} // get

			set {
				this.postcode = (string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()).Replace(" ", string.Empty);
			} // set
		} // Postcode

		/// <summary>
		/// Director address house number.
		/// </summary>
		public string HouseNumber {
			get {
				if (string.IsNullOrWhiteSpace(this.houseNumber))
					this.houseNumber = string.Empty;

				return this.houseNumber;
			} // get
			set {this.houseNumber = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();}
		} // HouseNumber

		public void SetAddress(string postCode, string line1, string line2) {
			Postcode = postCode;

			line1 = string.IsNullOrWhiteSpace(line1) ? string.Empty : line1.Trim();
			line2 = string.IsNullOrWhiteSpace(line2) ? string.Empty : line2.Trim();

			if (SetHouseNumber(line1))
				return;

			if (IsApartment(line1))
				SetHouseNumber(line2);
			else
				HouseNumber = line1;
		} // SetAddress

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/>
		/// is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object.</param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(this, obj))
				return true;

			var d = obj as DirectorData;

			if (d == null)
				return false;

			return
				(FirstName == d.FirstName) &&
				(LastName == d.LastName) &&
				(DateOfBirth == d.DateOfBirth) &&
				(Postcode == d.Postcode) &&
				(HouseNumber == d.HouseNumber);
		} // Equals

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return ToString().Replace(' ', '_').GetHashCode();
		} // GetHashCode

		public void Validate(List<string> result, string errorPrefix) {
			if (string.IsNullOrWhiteSpace(FirstName))
				result.Add(string.Format("{0} and no director first name specified.", errorPrefix));

			if (string.IsNullOrWhiteSpace(LastName))
				result.Add(string.Format("{0} and no director last name specified.", errorPrefix));

			if (DateOfBirth < longAgo) {
				result.Add(string.Format(
					"{0} and director is too old (born on {1}).",
					errorPrefix,
					DateOfBirth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
				));
			} // if

			if (string.IsNullOrWhiteSpace(Postcode))
				result.Add(string.Format("{0} and no director postcode specified.", errorPrefix));

			if (string.IsNullOrWhiteSpace(HouseNumber))
				result.Add(string.Format("{0} and no director house name/number specified.", errorPrefix));
		} // Validate

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0} {1} {2} at {3} ({4})",
				FirstName,
				LastName,
				DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				Postcode,
				string.IsNullOrWhiteSpace(HouseNumber) ? "unnumbered" : HouseNumber
			);
		} // ToString

		private bool SetHouseNumber(string addressLine) {
			if (addressLine.IndexOfAny(digits) != 0)
				return false;

			int spacePos = addressLine.IndexOf(" ", StringComparison.InvariantCulture);
			HouseNumber = spacePos > 0 ? addressLine.Substring(0, spacePos) : addressLine;

			return true;
		} // SetHouseNumber

		private static bool IsApartment(string addressLine) {
			foreach (string symptom in apartmentSymptoms) {
				if (addressLine.StartsWith(symptom + Space, StringComparison.InvariantCultureIgnoreCase))
					return true;

				if (addressLine.Equals(symptom, StringComparison.InvariantCultureIgnoreCase))
					return true;
			} // for each
			return false;
		} // IsApartment

		private string postcode;
		private string houseNumber;

		private const string Space = " ";
		private static readonly char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', };
		private static readonly string[] apartmentSymptoms = { "Flat", "Unit", "Po Box", "Apartment", "Suite", };
		private static readonly DateTime longAgo = DateTime.UtcNow.AddYears(-120);
	} // class DirectorData
} // namespace
