using System;
using EZBob.DatabaseLib.Model.Database.Repository;

namespace EzBob.Web.Code
{
    public class RefNumberGenerator
    {
        private readonly int _maxDigits = 6;
        private const string Prefix = "01";

        private readonly Random _rnd = new Random();
        private readonly ICustomerRepository _customers;

        protected virtual int GenerateRandomNumber (int digits)
        {
            return _rnd.Next(1, (int)Math.Pow(10, digits));
        }

        public RefNumberGenerator(ICustomerRepository customers)
        {
            _customers = customers;
        }

        public RefNumberGenerator(ICustomerRepository customers, int maxDigits)
        {
            _customers = customers;
            _maxDigits = maxDigits;
        }

        public string GenerateForCustomer()
        {
            var format = "{0:D" + _maxDigits + "}"; 
            var start = GenerateRandomNumber(_maxDigits);
            var maxNumber = (int) Math.Pow(10, _maxDigits);

            while (true)
            {
                var refnumber = Prefix + string.Format(format, start);
                var exists = _customers.RefNumberExists(refnumber);
                if (!exists) return refnumber;
                start = (start + 1) % maxNumber;
            }
        }
    }
}