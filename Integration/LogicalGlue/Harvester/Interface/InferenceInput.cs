namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Utils.Security;
	using Newtonsoft.Json;

	/// <summary>
	/// This data is sent to Logical Glue preprocessing service.
	/// </summary>
	[JsonConverter(typeof(InferenceInputSerializer))]
	public class InferenceInput {
		public string RequestID {
			get { return UniqueID.ToString("N").ToLowerInvariant(); }
		} // RequestID

		public Guid UniqueID {
			get {
				if (this.uniqueID == null)
					this.uniqueID = Guid.NewGuid();

				return this.uniqueID.Value;
			} // get
			set { this.uniqueID = value; } // set
		} // UniqueID

		/// <summary>
		/// Amount that customer requested. Not sent to Logical Glue, used in main strategy.
		/// </summary>
		public decimal RequestedAmount { get; set; }

		/// <summary>
		/// Term that customer requested. Not sent to Logical Glue, used in main strategy.
		/// </summary>
		public int RequestedTerm { get; set; }

		/// <summary>
		/// Data received from Equifax (via LG) on this company and director.
		/// </summary>
		/// <remarks>Can be empty (if no data was ever received).
		/// Overrides <see cref="CompanyRegistrationNumber"/> and <see cref="Director"/>.
		/// </remarks>
		public string EquifaxData { get; set; }

		/// <summary>
		/// LG returns answer related to this value (like in "what is probability that customer
		/// will repay loan if paying this amount every month").
		/// </summary>
		public decimal? MonthlyPayment { get; set; }

		/// <summary>
		/// Company identifier. Should be sent to LG as string.
		/// Limited company: its registration number (like 7852687 of EZBob Ltd).
		/// Non-limited company: unsupported.
		/// It should be company registration number in companieshouse.gov.uk.
		/// </summary>
		/// <remarks>Ignored when <see cref="EquifaxData" /> is contains value.</remarks>
		public string CompanyRegistrationNumber { get; set; }

		/// <summary>
		/// Director data (name, birth date).
		/// </summary>
		/// <remarks>Ignored when <see cref="EquifaxData" /> is contains value.</remarks>
		public DirectorData Director {
			get {
				if (this.director == null)
					this.director = new DirectorData();

				return this.director;
			} // get
			set { this.director = value ?? new DirectorData(); } // set
		} // Director

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/>
		/// is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(this, obj))
				return true;

			var ii = obj as InferenceInput;

			if (ii == null)
				return false;

			return
				(MonthlyPayment == ii.MonthlyPayment) &&
				(CompanyRegistrationNumber == ii.CompanyRegistrationNumber) &&
				Director.Equals(ii.Director) &&
				(EquifaxData == ii.EquifaxData);
		} // Equals

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return string.Join(
				"_",
				EquifaxData,
				MonthlyPayment == null ? "-- null --" : MonthlyPayment.Value.ToString(CultureInfo.InvariantCulture),
				CompanyRegistrationNumber,
				Director
			).GetHashCode();
		} // GetHashCode

		/// <summary>
		/// Returns a string that represents the current object.
		/// Equifax data is truncated up to 50 characters.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public string ToShortString() {
			string equifaxData = string.IsNullOrWhiteSpace(EquifaxData)
				? string.Empty
				: new Encrypted(EquifaxData.Length > 50 ? EquifaxData.Substring(0, 50) : EquifaxData);

			return string.Format(
				"Company: {0}, Payment {1}, Director: {2}, Equifax: {3}",
				CompanyRegistrationNumber,
				MonthlyPayment,
				Director,
				equifaxData
			);
		} // ToShortString

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"Company: {0}, Payment {1}, Director: {2}, Equifax: {3}",
				CompanyRegistrationNumber,
				MonthlyPayment,
				Director,
				EquifaxData
			);
		} // ToString

		public List<string> Validate() {
			var result = new List<string>();

			if (string.IsNullOrWhiteSpace(EquifaxData)) {
				if (string.IsNullOrWhiteSpace(CompanyRegistrationNumber))
					result.Add("No Equifax data and no company registration number are specified.");

				if (Director == null)
					result.Add("No Equifax data and no director are specified.");
				else
					Director.Validate(result, "No Equifax data and");
			} // if

			if ((MonthlyPayment ?? 0) <= 0) {
				result.Add(string.Format(
					"Monthly payment '{0}' is not positive.",
					MonthlyPayment == null ? "N/A" : MonthlyPayment.Value.ToString("C0", CultureInfo.InvariantCulture)
				));
			} // if

			return result.Count > 0 ? result : null;
		} // IsValid

		private DirectorData director;
		private Guid? uniqueID;
	} // class InferenceInput
} // namespace
