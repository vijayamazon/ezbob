namespace EzBob.Backend.Strategies.Misc
{
	using System.Data;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Backend.Models;

	internal class CustomerAddressHelper : AStrategy
	{
		public CustomerAddressHelper(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			_customerId = customerId;
		}

		public override string Name
		{
			get { return "CustomerAddressHelper"; }
		}

		public override void Execute()
		{
			CustomerAddresses = GetAddresses(_customerId);
		}

		private CustomerAddressesModel GetAddresses(int customerId)
		{
			DataTable dt = DB.ExecuteReader("GetCustomerAddresses", CommandSpecies.StoredProcedure,
			                                new QueryParameter("CustomerId", customerId));
			var addressesResults = new SafeReader(dt.Rows[0]);
			var model = new CustomerAddressesModel();
			model.CurrentAddress = new CustomerAddressModel
				{
					Line1 = addressesResults["Line1"],
					Line2 = addressesResults["Line2"],
					Line3 = addressesResults["Line3"],
					City = addressesResults["Line4"],
					County = addressesResults["Line5"],
					PostCode = addressesResults["Line6"],
				};
			model.PreviousAddress = new CustomerAddressModel
				{
					Line1 = addressesResults["Line1Prev"],
					Line2 = addressesResults["Line2Prev"],
					Line3 = addressesResults["Line3Prev"],
					City = addressesResults["Line4Prev"],
					County = addressesResults["Line5Prev"],
					PostCode = addressesResults["Line6Prev"],
				};

			FillAddress(model.CurrentAddress);
			FillAddress(model.PreviousAddress);
			return model;
		}

		public void FillAddress(CustomerAddressModel model)
		{
			var flatOrAppartmentNumber = string.Empty;
			var houseName = string.Empty;
			var houseNumber = string.Empty;
			var address1 = string.Empty;
			var address2 = string.Empty;
			var poBox = string.Empty;
			if (!string.IsNullOrEmpty(model.Line1) && string.IsNullOrEmpty(model.Line2) && string.IsNullOrEmpty(model.Line3))
			{
				houseNumber = Regex.Match(model.Line1, "\\d*").Value;
				if (!string.IsNullOrEmpty(houseNumber))
				{
					var line1 = model.Line1.Split(' ');
					houseNumber = line1[0];
					address1 = string.Join(" ", line1.Skip(1));
				}
				else if (model.Line1.ToUpper().StartsWith("PO BOX"))
				{
					poBox = model.Line1;
				}
				else
				{
					houseName = model.Line1;
				}
			}
			else if (!string.IsNullOrEmpty(model.Line1) && !string.IsNullOrEmpty(model.Line2) && string.IsNullOrEmpty(model.Line3))
			{
				houseNumber = Regex.Match(model.Line1, "\\d*").Value;
				if (!string.IsNullOrEmpty(houseNumber))
				{
					var line1 = model.Line1.Split(' ');
					houseNumber = line1[0];
					address1 = string.Join(" ", line1.Skip(1));

					address2 = model.Line2;
				}
				else if (model.Line1.ToUpper().StartsWith("APARTMENT") || model.Line1.ToUpper().StartsWith("FLAT"))
				{
					flatOrAppartmentNumber = model.Line1;
					houseNumber = Regex.Match(model.Line2, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = model.Line2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));
					}
					else
					{
						address1 = model.Line2;
					}
				}
				else
				{
					houseNumber = Regex.Match(model.Line2, "\\d*").Value;
					houseName = model.Line1;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = model.Line2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));
					}
					else
					{
						address1 = model.Line2;
					}
				}
			}
			else if ((!string.IsNullOrEmpty(model.Line1) && !string.IsNullOrEmpty(model.Line2) && !string.IsNullOrEmpty(model.Line3)))
			{
				if (model.Line1.ToUpper().StartsWith("APARTMENT") || model.Line1.ToUpper().StartsWith("FLAT"))
				{
					flatOrAppartmentNumber = model.Line1;
					houseNumber = Regex.Match(model.Line2, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line2 = model.Line2.Split(' ');
						houseNumber = line2[0];
						address1 = string.Join(" ", line2.Skip(1));

						address2 = model.Line3;
					}
					else
					{
						houseNumber = Regex.Match(model.Line3, "\\d*").Value;
						if (!string.IsNullOrEmpty(houseNumber))
						{
							var line3 = model.Line3.Split(' ');
							houseNumber = line3[0];
							address1 = string.Join(" ", line3.Skip(1));

						}
						else
						{
							address1 = model.Line3;
						}

						houseName = model.Line2;

					}
				}
				else
				{
					houseName = model.Line1;
					address1 = model.Line2;
					address2 = model.Line3;
				}
			}

			model.Address1 = address1;
			model.Address2 = address2;
			model.HouseName = houseName;
			model.HouseNumber = houseNumber;
			model.FlatOrApartmentNumber = flatOrAppartmentNumber;
			model.POBox = poBox;
		}

		private readonly int _customerId;
		public CustomerAddressesModel CustomerAddresses { get; set; }
	}
}
