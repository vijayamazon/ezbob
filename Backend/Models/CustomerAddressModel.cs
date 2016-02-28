namespace Ezbob.Backend.Models {
	using System.Linq;
	using System.Text.RegularExpressions;

	public class CustomerAddressModel {
		public int AddressId { get; set; }
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public string Line3 { get; set; }
		public string City { get; set; }
		public string County { get; set; }
		public string FlatOrApartmentNumber { get; set; }
		public string HouseNumber { get; set; }
		public string HouseName { get; set; }
		public string Country { get; set; }
		public string PostCode { get; set; }
		public string POBox { get; set; }

		public string Address1 { get; set; }
		public string Address2 { get; set; }

		public override string ToString() {
			return string.Format(
				"1: {0}, 2: {1}, 3: {2}, h# {3}, hn: {4} fa#: {5}",
				Line1,
				Line2,
				Line3,
				HouseNumber,
				HouseName,
				FlatOrApartmentNumber
			);
		} // ToString

		public void FillDetails() {
			var filled = new CustomerAddressModel {
				FlatOrApartmentNumber = string.Empty,
				HouseName = string.Empty,
				HouseNumber = string.Empty,
				Address1 = string.Empty,
				Address2 = string.Empty,
				POBox = string.Empty,
			};

			if (!string.IsNullOrWhiteSpace(Line1)) {
				if (!string.IsNullOrWhiteSpace(Line2)) {
					if (!string.IsNullOrWhiteSpace(Line3))
						FillFromThreeLines(filled);
					else
						FillFromTwoLines(filled);
				} else
					FillFromOneLine(filled);
			} // if

			Address1 = filled.Address1;
			Address2 = filled.Address2;
			HouseName = filled.HouseName;
			HouseNumber = filled.HouseNumber;
			FlatOrApartmentNumber = filled.FlatOrApartmentNumber;
			POBox = filled.POBox;
		} // FillDetails

		private void FillFromOneLine(CustomerAddressModel filled) {
			filled.HouseNumber = Regex.Match(Line1, "\\d*").Value;

			if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
				var line1 = Line1.Split(' ');
				filled.HouseNumber = line1[0];
				filled.Address1 = string.Join(" ", line1.Skip(1));
			} else if (Line1.ToUpper().StartsWith("PO BOX"))
				filled.POBox = Line1;
			else
				filled.HouseName = Line1;
		} // FillFromOneLine

		private void FillFromTwoLines(CustomerAddressModel filled) {
			filled.HouseNumber = Regex.Match(Line1, "\\d*").Value;

			if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
				var line1 = Line1.Split(' ');
				filled.HouseNumber = line1[0];
				filled.Address1 = string.Join(" ", line1.Skip(1));
				filled.Address2 = Line2;
				return;
			} // if

			string uLine1 = Line1.ToUpper();

			if (uLine1.StartsWith("APARTMENT") || uLine1.StartsWith("FLAT")) {
				filled.FlatOrApartmentNumber = Line1;
				filled.HouseNumber = Regex.Match(Line2, "\\d*").Value;

				if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
					var line2 = Line2.Split(' ');
					filled.HouseNumber = line2[0];
					filled.Address1 = string.Join(" ", line2.Skip(1));
				} else
					filled.Address1 = Line2;
			} else {
				filled.HouseNumber = Regex.Match(Line2, "\\d*").Value;
				filled.HouseName = Line1;

				if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
					var line2 = Line2.Split(' ');
					filled.HouseNumber = line2[0];
					filled.Address1 = string.Join(" ", line2.Skip(1));
				} else
					filled.Address1 = Line2;
			} // if
		} // FillFromTwoLines

		private void FillFromThreeLines(CustomerAddressModel filled) {
			string uLine1 = Line1.ToUpper();

			if (uLine1.StartsWith("APARTMENT") || uLine1.StartsWith("FLAT")) {
				filled.FlatOrApartmentNumber = Line1;
				filled.HouseNumber = Regex.Match(Line2, "\\d*").Value;

				if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
					string[] line2 = Line2.Split(' ');
					filled.HouseNumber = line2[0];
					filled.Address1 = string.Join(" ", line2.Skip(1));
					filled.Address2 = Line3;
				} else {
					filled.HouseNumber = Regex.Match(Line3, "\\d*").Value;

					if (!string.IsNullOrWhiteSpace(filled.HouseNumber)) {
						string[] line3 = Line3.Split(' ');
						filled.HouseNumber = line3[0];
						filled.Address1 = string.Join(" ", line3.Skip(1));
					} else
						filled.Address1 = Line3;

					filled.HouseName = Line2;
				} // if

				return;
			} // if

			bool neitherUnitNorBlock =
				!uLine1.Contains("UNIT") &&
				!uLine1.Contains("BLOCK") &&
				Regex.Match(Line1, "^\\d[0-9a-zA-Z ]*$").Success;
			
			if (neitherUnitNorBlock) {
				filled.HouseNumber = Regex.Match(Line1, "\\d*").Value;

				if (string.IsNullOrWhiteSpace(filled.HouseNumber))
					return;

				var line1 = Line1.Split(' ');
				filled.HouseNumber = line1[0];
				filled.Address1 = string.Join(" ", line1.Skip(1));
				filled.Address2 = Line2;
			} else {
				filled.HouseName = Line1;
				filled.Address1 = Line2;
				filled.Address2 = Line3;
			} // if
		} // FillFromThreeLines
	} // class CustomerAddressModel
} // namespace
