namespace EzBobModels.Customer
{
    using System;

    public class CustomerSession
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartSession { get; set; }
        public string Ip { get; set; }
        public bool IsPasswdOk { get; set; }
        public string ErrorMessage { get; set; }
    }
}
