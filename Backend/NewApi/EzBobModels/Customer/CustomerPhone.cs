namespace EzBobModels.Customer
{
    using System;

    public class CustomerPhone
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public string PhoneType { get; set; }
        public string Phone { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public bool IsCurrent { get; set; }
    }
}
