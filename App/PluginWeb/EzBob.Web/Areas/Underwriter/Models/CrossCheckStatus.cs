using System;
using System.Linq;
using ApplicationMng.Model;

namespace EzBob.Web.Areas.Underwriter.Models
{
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
            Email = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.Empty, eBay.Email, application.Email, payPal.Email);
            FirstName = GetTypeStatusForThreeColums(eBay.FirstName, application.FirstName, payPal.FirstName, CrossCheckTypeStatus.NoChecked);
            Surname = GetTypeStatusForThreeColums(eBay.Surname, application.Surname, payPal.Surname, CrossCheckTypeStatus.NoChecked);
            FullName = GetStatusForFullName(FirstName, Surname);
            DateOfBirth = GetTypeStatusForTwoColumns(application.DateOfBirth.ToString(), payPal.DateOfBirth.ToString(), CrossCheckTypeStatus.NoChecked);
            DaytimePhone = GetStatusDayTimePhone(eBay.DaytimePhone, application.DaytimePhone, payPal.DaytimePhone, CrossCheckTypeStatus.Empty);
            TypeOfBussnes = GetTypeStatusForTwoColumns(application.TypeOfBusiness.ToString(), eBay.SellerInfo.SellerInfoSellerBusinessType, CrossCheckTypeStatus.Empty);
        }

        public void BuildMarkerStatusForCustomerAddress(CustomerAddress current, CustomerAddress ebaySeller, CustomerAddress payPall)
        {
            payPall = payPall ?? new CustomerAddress();
            ebaySeller = ebaySeller ?? new CustomerAddress();
            current = current ?? new CustomerAddress();
            Line1 = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.Line1, current.Line1, payPall.Line1);
            Line2 = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.Line2, current.Line2, payPall.Line2);
            Town = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.Town, current.Town, payPall.Town);
            County = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.County, current.County, payPall.County);
            Country = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.Country, current.Country, payPall.Country);
            Postcode = GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus.NoChecked, ebaySeller.Postcode, current.Postcode, payPall.Postcode);
        }

        public CrossCheckTypeStatus GetStatusDayTimePhone(string eBayDaytimePhone, string applicationDaytimePhone, string payPalDaytimePhone, CrossCheckTypeStatus defaultStatus)
        {
            applicationDaytimePhone = applicationDaytimePhone.Replace("+3", string.Empty);

            var result = defaultStatus;

            if (!String.IsNullOrEmpty(eBayDaytimePhone) && !String.IsNullOrEmpty(payPalDaytimePhone))
                result = GetTypeStatusForThreeColums(eBayDaytimePhone, applicationDaytimePhone, payPalDaytimePhone, defaultStatus);
            else
            {
                if (String.IsNullOrEmpty(eBayDaytimePhone))
                    result = GetTypeStatusForTwoColumns(applicationDaytimePhone, payPalDaytimePhone, CrossCheckTypeStatus.NoChecked);
                else
                    if (String.IsNullOrEmpty(payPalDaytimePhone))
                        result = GetTypeStatusForTwoColumns(applicationDaytimePhone, eBayDaytimePhone, CrossCheckTypeStatus.NoChecked);
            }
            return result;
        }

        public CrossCheckTypeStatus GetStatusForFullName(CrossCheckTypeStatus statusFirstName, CrossCheckTypeStatus statusSurname)
        {
            return statusFirstName == CrossCheckTypeStatus.Checked && statusSurname == CrossCheckTypeStatus.Checked
                       ? CrossCheckTypeStatus.Checked
                       : CrossCheckTypeStatus.NoChecked;
        }

        public CrossCheckTypeStatus GetTypeStatusForThreeColumsAll(CrossCheckTypeStatus defaultStatus, params string[] values)
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
                return defaultStatus;

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

            return allEqual ? CrossCheckTypeStatus.Checked : defaultStatus;
        }

        public CrossCheckTypeStatus GetTypeStatusForTwoColumns(string applicationName, string payPallName, CrossCheckTypeStatus status)
        {
            if (String.IsNullOrEmpty(applicationName) || String.IsNullOrEmpty(payPallName))
                return status;

            applicationName = applicationName.Replace(" ", string.Empty);
            payPallName = payPallName.Replace(" ", string.Empty);

            var isequal = string.Equals(applicationName, payPallName, StringComparison.OrdinalIgnoreCase);

            return isequal ? CrossCheckTypeStatus.Checked : status;
        }

        public CrossCheckTypeStatus GetTypeStatusForThreeColums(string ebayName, string applicationName, string payPallName, CrossCheckTypeStatus defaultStatus)
        {
            return String.IsNullOrEmpty(ebayName) ?
                GetTypeStatusForTwoColumns(applicationName, payPallName, CrossCheckTypeStatus.NoChecked)
                : GetTypeStatusForThreeColumsAll(defaultStatus, ebayName, applicationName, payPallName);
        }
    }
}