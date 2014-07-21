namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.ExperianParser;
	using Ezbob.Utils.Security;

	#region enum ParsedFieldNames

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

	#endregion enum ParsedFieldNames

	#region class ExperianDirector

	internal class ExperianDirector {
		#region constructor

		#region constructor for limited director

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

		#endregion constructor for limited director

		#region constructor for limited shareholder

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

		#endregion constructor for limited shareholder

		#region constructor for limited shareholder

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

		#endregion constructor for limited shareholder

		#region constructor for non-limited director/shareholder

		public ExperianDirector(ParsedDataItem oItem, int nCustomerID, bool bIsDirector) {
			IsValid = false;

			bool bIsCompany = oItem.Get(ParsedFieldNames.IsCompany).Equals("Y", StringComparison.InvariantCultureIgnoreCase);

			if (bIsCompany)
				return;

			CustomerID = nCustomerID;

			IsDirector = bIsDirector;
			IsShareholder = !bIsDirector;

			FirstName = oItem.Get(ParsedFieldNames.FirstName);

			if (FirstName == string.Empty)
				return;

			LastName = oItem.Get(ParsedFieldNames.LastName);

			if (LastName == string.Empty)
				return;

			switch (oItem.Get(ParsedFieldNames.Prefix).ToUpper(CultureInfo.InvariantCulture)) {
			case "MR":
				Gender = 'M';
				break;

			case "MRS":
			case "MS":
				Gender = 'F';
				break;
			} // switch

			Town = oItem.Get(ParsedFieldNames.Town);
			County = oItem.Get(ParsedFieldNames.County);
			Postcode = oItem.Get(ParsedFieldNames.Postcode);

			if (IsDirector) {
				if (Town == string.Empty)
					return;

				DateTime d;

				if (DateTime.TryParseExact(oItem.Get(ParsedFieldNames.BirthDate), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
					BirthDate = d;
				else
					return;

				if (BirthDate < ms_oLongAgo) {
					BirthDate = null;
					return;
				} // if

				MiddleName = oItem.Get(ParsedFieldNames.MidName1, ParsedFieldNames.MidName2);

				var lines = new List<string>();

				var sHouseName = oItem.Get(ParsedFieldNames.HouseName);
				if (sHouseName != string.Empty)
					lines.Add(sHouseName);

				var sAddr = oItem.Get(ParsedFieldNames.HouseNumber, ParsedFieldNames.Street);
				if (sAddr != string.Empty)
					lines.Add(sAddr);

				Line1 = lines.Count > 0 ? lines[0] : string.Empty;

				if (Line1 == string.Empty)
					return;

				Line2 = lines.Count > 1 ? lines[1] : string.Empty;
				Line3 = string.Empty;

				RefNum = oItem.Get(ParsedFieldNames.Number);
			}
			else {
				MiddleName = oItem.Get(ParsedFieldNames.MidName1);

				Line1 = oItem.Get(ParsedFieldNames.AddressLine1);
				Line2 = oItem.Get(ParsedFieldNames.AddressLine2);
				Line3 = oItem.Get(ParsedFieldNames.AddressLine3);
			} // if

			IsValid = true;
		} // constructor

		#endregion constructor for non-limited director/shareholder

		#endregion constructor

		#region property IsValid

		public bool IsValid { get; private set; } // IsValid

		#endregion property IsValid

		#region property FullName

		public string FullName { get { return FirstName + " " + LastName; } } // FullName

		#endregion property FullName

		#region method ToString

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

		#endregion method ToString

		#region DB fields

		public int CustomerID { get; set; }

		#region property RefNum

		public string RefNum {
			get {
				if (string.IsNullOrWhiteSpace(m_sRefNum)) {
					m_sRefNum = "EZ" + SecurityUtils.Hash(
						FirstName.ToUpperInvariant() + " " +
						MiddleName.ToUpperInvariant() + " " +
						LastName.ToUpperInvariant() + " " +
						(BirthDate.HasValue ? BirthDate.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : string.Empty)
					);
				} // if

				return m_sRefNum;
			} // get
			set { m_sRefNum = value; }
		} // RefNum

		private string m_sRefNum;

		#endregion property RefNum

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

		#endregion DB fields

		#region private

		private static readonly DateTime ms_oLongAgo = new DateTime(1775, 5, 7);

		private static string TwoString(string a, string b) {
			return Ti(((a ?? string.Empty).Trim() + " " + (b ?? string.Empty).Trim()).Trim());
		} // TwoString

		private static string Ti(string a) {
			return CultureInfo.InvariantCulture.TextInfo.ToTitleCase((a ?? string.Empty).Trim().ToLowerInvariant());
		} // Ti

		#endregion private
	} // class ExperianDirector

	#endregion class ExperianDirector

	#region class ParsedDataItemExt

	internal static class ParsedDataItemExt {
		public static string Get(this ParsedDataItem oItem, ParsedFieldNames nNameA, ParsedFieldNames nNameB) {
			return (oItem.Get(nNameA) + " " + oItem.Get(nNameB)).Trim();
		} // Get

		public static string Get(this ParsedDataItem oItem, ParsedFieldNames nName) {
			string sKey = nName.ToString();

			string sValue = ((oItem.Contains(sKey) ? oItem[sKey] : string.Empty) ?? string.Empty).Trim().ToLowerInvariant();

			if (nName == ParsedFieldNames.Postcode)
				return sValue.ToUpperInvariant();

			return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sValue);
		} // Get
	} // class ParsedDataItemExt

	#endregion class ParsedDataItemExt
} // namespace
