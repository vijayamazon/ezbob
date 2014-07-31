using System;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;

	public enum CrossCheckTypeStatus
	{
		Empty,
		Checked,
		NoChecked
	}

	public class CrossCheckStatus
	{
		public CrossCheckTypeStatus DateOfBirth { get; set; }
		public CrossCheckTypeStatus MiddleInitial { get; set; }
		public CrossCheckTypeStatus DaytimePhone { get; set; }
		public CrossCheckTypeStatus BusinessName { get; set; }
		public CrossCheckTypeStatus TypeOfBussnes { get; set; }
		public CrossCheckTypeStatus FirstName { get; set; }
		public CrossCheckTypeStatus Surname { get; set; }
		public CrossCheckTypeStatus FullName { get; set; }
		public CrossCheckTypeStatus Email { get; set; }

		public CrossCheckTypeStatus Line1 { get; set; }
		public CrossCheckTypeStatus Line2 { get; set; }
		public CrossCheckTypeStatus Line3 { get; set; }
		public CrossCheckTypeStatus Town { get; set; }
		public CrossCheckTypeStatus County { get; set; }
		public CrossCheckTypeStatus Postcode { get; set; }
		public CrossCheckTypeStatus Country { get; set; }

		public void BuildMarkerStatusForPersonalInfo(PersonalInfo application, PersonalInfo payPal, PersonalInfo eBay)
		{
			payPal = payPal ?? new PersonalInfo();
			eBay = eBay ?? new PersonalInfo();
			Email = GetTypeStatusForColums(eBay.Email, application.Email, payPal.Email);
			FirstName = GetTypeStatusForColums(eBay.FirstName, application.FirstName, payPal.FirstName);
			Surname = GetTypeStatusForColums(eBay.Surname, application.Surname, payPal.Surname);
			FullName = GetStatusForFullName(FirstName, Surname);
			DateOfBirth = GetTypeStatusForColums(application.DateOfBirth.ToString(), payPal.DateOfBirth.ToString());
			DaytimePhone = GetStatusDayTimePhone(eBay.DaytimePhone, application.DaytimePhone, payPal.DaytimePhone, CrossCheckTypeStatus.Empty);
			TypeOfBussnes = GetTypeStatusForColums(application.TypeOfBusiness.ToString(), eBay.SellerInfo.SellerInfoSellerBusinessType);
		}

		public void BuildMarkerStatusForCustomerAddress(CustomerAddress current, CustomerAddress ebaySeller, CustomerAddress payPal)
		{
			payPal = payPal ?? new CustomerAddress();
			ebaySeller = ebaySeller ?? new CustomerAddress();
			current = current ?? new CustomerAddress();
			Line1 = GetTypeStatusForColums(ebaySeller.Line1, current.Line1, payPal.Line1);
			Line2 = GetTypeStatusForColums(ebaySeller.Line2, current.Line2, payPal.Line2);
			Town = GetTypeStatusForColums(ebaySeller.Town, current.Town, payPal.Town);
			County = GetTypeStatusForColums(ebaySeller.County, current.County, payPal.County);
			Country = GetTypeStatusForCountry(ebaySeller.Country, current.Country, payPal.Country);
			Postcode = GetTypeStatusForColums(ebaySeller.Postcode, current.Postcode, payPal.Postcode);
		}

		public CrossCheckTypeStatus GetStatusDayTimePhone(string eBayDaytimePhone, string applicationDaytimePhone, string payPalDaytimePhone, CrossCheckTypeStatus defaultStatus)
		{
			if (applicationDaytimePhone != null)
			{
				applicationDaytimePhone = applicationDaytimePhone.Replace("+3", string.Empty);
			}
			return GetTypeStatusForColums(eBayDaytimePhone, applicationDaytimePhone, payPalDaytimePhone);
		}

		public CrossCheckTypeStatus GetStatusForFullName(CrossCheckTypeStatus statusFirstName, CrossCheckTypeStatus statusSurname)
		{
			if (statusFirstName == CrossCheckTypeStatus.Checked && statusSurname == CrossCheckTypeStatus.Checked)
			{
				return CrossCheckTypeStatus.Checked;
			}
			if (statusFirstName == CrossCheckTypeStatus.NoChecked || statusSurname == CrossCheckTypeStatus.NoChecked)
			{
				return CrossCheckTypeStatus.NoChecked;
			}

			return CrossCheckTypeStatus.Empty;
		}

		public CrossCheckTypeStatus GetTypeStatusForCountry(params string[] values)
		{
			var uk = new[]
            {
                "uk",
                "unitedkingdom",
                "england",
                "gb",
                "greatbritain"
            };

			var stripped = values.Where(v => !string.IsNullOrEmpty(v)).ToList();

			if (stripped.Count < 2)
				return CrossCheckTypeStatus.Empty;

			var first = stripped.First().Replace(" ", string.Empty).ToLower();

			var allEqual = stripped.All(v =>
										  {
											  var vv = v.Replace(" ", string.Empty).ToLower();
											  if (vv == first)
											  {
												  return true;
											  }

											  if (uk.Contains(first) && uk.Contains(vv)) return true;

											  return false;

										  });

			return allEqual ? CrossCheckTypeStatus.Checked : CrossCheckTypeStatus.NoChecked;
		}

		public CrossCheckTypeStatus GetTypeStatusForColums(params string[] names)
		{
			var nonEmptyNames = new List<string>();
			foreach (var name in names)
			{
				if (!string.IsNullOrEmpty(name)) { nonEmptyNames.Add(name.Trim()); }	
			}
			
			if (nonEmptyNames.Count < 2)
			{
				return CrossCheckTypeStatus.Empty;
			}

			return GetTypeStatusForList(nonEmptyNames);

		}

		private CrossCheckTypeStatus GetTypeStatusForList(List<string> names)
		{
			var name1 = names[0];
			foreach (var name in names)
			{
				if (!string.Equals(name1, name, StringComparison.OrdinalIgnoreCase))
				{
					return CrossCheckTypeStatus.NoChecked;
				}
			}
			return CrossCheckTypeStatus.Checked;
		}
	}
}