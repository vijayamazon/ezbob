namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Utils.Security;

	internal enum ParsedFieldNames {
		AddressLine1,
		AddressLine2,
		AddressLine3,
		BirthDate,
		Country,
		County,
		FirstName,
		HouseName,
		HouseNumber,
		IsCompany,
		LastName,
		MidName1,
		MidName2,
		Number,
		Postcode,
		Prefix,
		ShareInfo,
		Street,
		Suffix,
		Title,
		Town,
	}

	// enum ParsedFieldNames

	internal class ExperianDirector {

		public ExperianDirector(ExperianLtdDL72 oItem, int nCustomerID) {
			IsValid = false;

			if (
				!oItem.BirthDate.HasValue ||
				(oItem.BirthDate < ms_oLongAgo) ||
				string.IsNullOrWhiteSpace(oItem.FirstName) ||
				string.IsNullOrWhiteSpace(oItem.FirstName) ||
				string.IsNullOrWhiteSpace(oItem.Town) ||
				oItem.IsCompany.Equals("Y", StringComparison.InvariantCultureIgnoreCase)
			)
				return;

			var lines = new List<string>();

			var sHouseName = Ti(oItem.HouseName);
			if (sHouseName != string.Empty)
				lines.Add(sHouseName);

			var sAddr = TwoString(oItem.HouseNumber, oItem.Street);
			if (sAddr != string.Empty)
				lines.Add(sAddr);

			Line1 = lines.Count > 0 ? lines[0] : string.Empty;

			if (Line1 == string.Empty)
				return;

			Line2 = lines.Count > 1 ? lines[1] : string.Empty;
			Line3 = string.Empty;

			FirstName = Ti(oItem.FirstName);

			LastName = Ti(oItem.LastName);

			Town = Ti(oItem.Town);

			BirthDate = oItem.BirthDate;

			RefNum = oItem.Number;

			MiddleName = TwoString(oItem.MidName1, oItem.MidName2);
			County = Ti(oItem.County);
			Postcode = Ti(oItem.Postcode).ToUpperInvariant();

			switch (oItem.Prefix.ToUpper(CultureInfo.InvariantCulture)) {
			case "MR":
				Gender = 'M';
				break;

			case "MRS":
			case "MS":
				Gender = 'F';
				break;
			} // switch

			CustomerID = nCustomerID;

			IsDirector = true;
			IsShareholder = false;

			IsValid = true;
		} // constructor

		public ExperianDirector(ExperianLtdDLB5 oItem, int nCustomerID) {
			IsValid = false;

			if (string.IsNullOrWhiteSpace(oItem.FirstName) || string.IsNullOrWhiteSpace(oItem.LastName))
				return;

			CustomerID = nCustomerID;

			IsDirector = false;
			IsShareholder = true;

			FirstName = Ti(oItem.FirstName);

			LastName = Ti(oItem.LastName);

			switch (oItem.Prefix.ToUpper(CultureInfo.InvariantCulture)) {
			case "MR":
				Gender = 'M';
				break;

			case "MRS":
			case "MS":
				Gender = 'F';
				break;
			} // switch

			Town = Ti(oItem.Town);
			County = Ti(oItem.County);
			Postcode = Ti(oItem.Postcode).ToUpperInvariant();

			MiddleName = Ti(oItem.MidName1);

			Line1 = Ti(oItem.AddressLine1);
			Line2 = Ti(oItem.AddressLine2);
			Line3 = Ti(oItem.AddressLine3);

			IsValid = true;
		} // constructor

		public ExperianDirector(string sFullName, int nCustomerID) {
			IsValid = false;

			if (string.IsNullOrWhiteSpace(sFullName))
				return;

			sFullName = sFullName.Trim();

			int nFirst = sFullName.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase);

			if (nFirst < 0)
				return;

			FirstName = sFullName.Substring(0, nFirst);

			if (string.IsNullOrWhiteSpace(FirstName))
				return;

			int nLast = sFullName.LastIndexOf(" ", sFullName.Length - 1, sFullName.Length - nFirst - 1, StringComparison.InvariantCultureIgnoreCase);

			if (nLast < 0) {
				LastName = sFullName.Substring(nFirst + 1);
				MiddleName = string.Empty;
			}
			else {
				LastName = sFullName.Substring(nLast + 1);
				MiddleName = sFullName.Substring(nFirst + 1, nLast - nFirst - 1);
			} // if

			if (string.IsNullOrWhiteSpace(LastName))
				return;

			FirstName = Ti(FirstName);
			MiddleName = Ti(MiddleName);
			LastName = Ti(LastName);

			CustomerID = nCustomerID;

			IsDirector = false;
			IsShareholder = true;

			Town = string.Empty;
			County = string.Empty;
			Postcode = string.Empty;

			Line1 = string.Empty;
			Line2 = string.Empty;
			Line3 = string.Empty;

			IsValid = true;
		} // constructor

		public bool IsValid { get; private set; } // IsValid

		public string FullName { get { return FirstName + " " + LastName; } } // FullName

		public override string ToString() {
			string sType = string.Empty;

			if (IsDirector && IsShareholder)
				sType = "Dirsha ";
			else if (IsDirector)
				sType = "Director ";
			else if (IsShareholder)
				sType = "Shareholder ";

			return sType + FullName;
		} // ToString

		public int CustomerID { get; set; }

		public string RefNum {
			get {
				if (string.IsNullOrWhiteSpace(m_sRefNum)) {
					m_sRefNum = "EZ" + PasswordUtility.HashPasswordOldWay(
						FirstName.ToUpperInvariant() + " " +
						MiddleName.ToUpperInvariant() + " " +
						LastName.ToUpperInvariant() + " " +
						(BirthDate.HasValue
							? BirthDate.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture)
							: string.Empty
						)
					);
				} // if

				return m_sRefNum;
			} // get
			set { m_sRefNum = value; }
		} // RefNum

		private string m_sRefNum;

		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }

		public DateTime? BirthDate { get; set; }

		public char? Gender { get; set; }

		public bool IsDirector { get; set; }
		public bool IsShareholder { get; set; }

		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public string Line3 { get; set; }
		public string Town { get; set; }
		public string County { get; set; }
		public string Postcode { get; set; }

		private static readonly DateTime ms_oLongAgo = new DateTime(1775, 5, 7);

		private static string TwoString(string a, string b) {
			return Ti(((a ?? string.Empty).Trim() + " " + (b ?? string.Empty).Trim()).Trim());
		} // TwoString

		private static string Ti(string a) {
			return CultureInfo.InvariantCulture.TextInfo.ToTitleCase((a ?? string.Empty).Trim().ToLowerInvariant());
		} // Ti

	} // class ExperianDirector

} // namespace
