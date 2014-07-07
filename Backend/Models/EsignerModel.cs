namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Runtime.Serialization;
	using System.Text.RegularExpressions;
	using Utils;

	[DataContract]
	public class Esigner : ITraversable {
		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public long ID { get; set; }

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public int DirectorID { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public int StatusID { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public DateTime? SignDate { get; set; }

		[DataMember]
		public bool IsDirector { get; set; }

		[DataMember]
		public bool IsShareholder { get; set; }

		[DataMember]
		public string MobilePhone { get; set; }

		[DataMember]
		public string Line1 { get; set; }

		[DataMember]
		public string Line2 { get; set; }

		[DataMember]
		public string Line3 { get; set; }

		[DataMember]
		public string Town { get; set; }

		[DataMember]
		public string County { get; set; }

		[DataMember]
		public string Postcode { get; set; }

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} {3} {4}, {5} {6}, {7}",
				ID, DirectorID, FirstName, LastName, Email,
				Status, StatusID,
				SignDate.HasValue ? "signed on " + SignDate.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture) : "not signed"
			);
		} // ToString

		public string ValidateExperianDirectorDetails() {
			var err = new List<string>();

			if (DirectorID < 0)
				err.Add("Director ID not specified.");

			if (string.IsNullOrWhiteSpace(Email) || !ms_reEmail.IsMatch(Email))
				err.Add("Email not specified.");

			if (string.IsNullOrWhiteSpace(MobilePhone) || !ms_rePhone.IsMatch(MobilePhone))
				err.Add("Mobile phone not specified.");

			if (string.IsNullOrWhiteSpace(Line1))
				err.Add("Address line 1 not specified.");

			if (string.IsNullOrWhiteSpace(Town))
				err.Add("Town not specified.");

			if (string.IsNullOrWhiteSpace(Postcode))
				err.Add("Postcode not specified.");

			return err.Count < 1 ? null : string.Join(" ", err);
		} // ValidateExperianDirectorDetails

		private static readonly Regex ms_reEmail = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-_]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase);
		private static readonly Regex ms_rePhone = new Regex(@"^0[0-9]{10}$");
	} // class Esigner
} // namespace
