namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Globalization;
	using Newtonsoft.Json;

	/// <summary>
	/// This data is sent to Logical Glue preprocessing service.
	/// </summary>
	[JsonConverter(typeof(InferenceInputSerializer))]
	public class InferenceInput {
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
		public decimal MonthlyPayment { get; set; }

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
				MonthlyPayment.ToString(CultureInfo.InvariantCulture),
				CompanyRegistrationNumber,
				Director
			).GetHashCode();
		} // GetHashCode

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

				return (FirstName == d.FirstName) && (LastName == d.LastName) && (DateOfBirth == d.DateOfBirth);
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

			public bool IsValid() {
				if (string.IsNullOrWhiteSpace(FirstName))
					return false;

				if (string.IsNullOrWhiteSpace(LastName))
					return false;

				if (DateOfBirth < longAgo)
					return false;

				return true;
			} // IsValid

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"{0} {1} {2}",
					FirstName,
					LastName,
					DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
				);
			} // ToString

			private static readonly DateTime longAgo = DateTime.UtcNow.AddMonths(-12 * 120);
		} // class DirectorData

		public bool IsValid() {
			if (string.IsNullOrWhiteSpace(EquifaxData)) {
				if (string.IsNullOrWhiteSpace(CompanyRegistrationNumber))
					return false;

				if (Director == null)
					return false;

				if (!Director.IsValid())
					return false;
			} // if

			if (MonthlyPayment <= 0)
				return false;

			return true;
		} // IsValid

		private DirectorData director;
	} // class InferenceInput
} // namespace
