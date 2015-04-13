namespace Ezbob.CreditSafeLib
{
    public class CreditsafeRequestModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Operation { get; set; }
        public string ChargeReference { get; set; }
        public string Package { get; set; }
        public string Country { get; set; }
        public string companynumber { get; set; }
    }
}
