using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalAccountInfoModel
    {
        public PayPalAccountInfoModel(MP_PayPalPersonalInfo pi)
        {
            if (pi == null)
            {
                Empty();
                return;
            }
                
            FullName = pi.FullName;
            DateOfBirth = pi.DateOfBirth.HasValue ? pi.DateOfBirth.ToString() : "-";
            Email = pi.EMail;
            Phone = Phone;
            BuisnessName = pi.BusinessName;
            Street2 = pi.Street2;
            City = pi.City;
            State = pi.State;
            Postcode = pi.Postcode;
            Country = pi.Country;
        }

        public PayPalAccountInfoModel()
        {
            Empty();
        }

        private void Empty()
        {
            FullName = "-";
            DateOfBirth = "-";
            Email = "-";
            Phone = "-";
            BuisnessName = "-";
            Street2 = "-";
            City = "-";
            State = "-";
            Postcode = "-";
            Country = "-";
        }

        public string FullName { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BuisnessName { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
    }
}