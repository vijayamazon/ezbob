namespace EzBob.Backend.Strategies.Misc
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Backend.Models;

	public class CustomerAddressHelper : AStrategy
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
			OwnedAddresses = GetAddresses(_customerId);
		}

		private List<CustomerAddressModel> GetAddresses(int customerId)
		{
			if (customerId == 0)
			{
				return null;
			}

			List<CustomerAddressModel> ownedAddresses = DB.Fill<CustomerAddressModel>("GetCustomerAddresses",
			                                                                          CommandSpecies.StoredProcedure,
			                                                                          new QueryParameter("CustomerId", customerId));
			foreach (CustomerAddressModel address in ownedAddresses)
			{
				FillAddress(address);
			}

			return ownedAddresses;
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
				else if (Regex.Match(model.Line1, "^\\d[0-9a-zA-Z ]*$").Success && !model.Line1.ToUpper().Contains("UNIT") && !model.Line1.ToUpper().Contains("BLOCK"))
				{
					houseNumber = Regex.Match(model.Line1, "\\d*").Value;
					if (!string.IsNullOrEmpty(houseNumber))
					{
						var line1 = model.Line1.Split(' ');
						houseNumber = line1[0];
						address1 = string.Join(" ", line1.Skip(1));
						address2 = model.Line2;
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
		public List<CustomerAddressModel> OwnedAddresses { get; set; }
	}
}
