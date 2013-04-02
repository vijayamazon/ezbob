using System;

namespace Code.PayPal
{
    public class PersonalData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OpenIdNetContactFullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string CompanyName { get; set; }
        public string ContactCityhome { get; set; }
        public string ContactCountryhome { get; set; }
        public string ContactEmail { get; set; }
        public string Contactphonedefault { get; set; }
    }
}