using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class IdHubCustomAddressModel
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }

        public string CurAddressLine1 { get; set; }
        public string CurAddressLine2 { get; set; }
        public string CurAddressLine3 { get; set; }
        public string CurAddressTown { get; set; }
        public string CurAddressCounty { get; set; }
        public string CurAddressPostcode { get; set; }
        public string CurAddressCountry { get; set; }

        public string PrevAddressLine1 { get; set; }
        public string PrevAddressLine2 { get; set; }
        public string PrevAddressLine3 { get; set; }
        public string PrevAddressTown { get; set; }
        public string PrevAddressCounty { get; set; }
        public string PrevAddressPostcode { get; set; }
        public string PrevAddressCountry { get; set; }

        public string BankAccount { get; set; }
        public string SortCode { get; set; }

        public string IdHubAddressHouseNumber { get; set; }
        public string IdHubAddressHouseName { get; set; }
        public string IdHubAddressStreet { get; set; }
        public string IdHubAddressDistrict { get; set; }
        public string IdHubAddressTown { get; set; }
        public string IdHubAddressCounty { get; set; }
        public string IdHubAddressPostcode { get; set; }
        public string IdHubAddressCountry { get; set; }
    }
}