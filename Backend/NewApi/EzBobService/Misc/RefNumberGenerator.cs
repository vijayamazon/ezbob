using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.Misc {
    using EzBobCommon;
    using EzBobPersistence.Customer;

    [Obsolete("!!Queries DB !! Ref Number Generation should be changed to something that do not depend on DB (and not limited to 6 digits ?)")]
    public class RefNumberGenerator {
        private readonly int _maxDigits = 6;
        private const string Prefix = "01";

        private readonly Random _rnd = new Random();

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        public string GenerateRefNumber() {
            var format = "{0:D" + _maxDigits + "}";
            var start = GenerateRandomNumber(_maxDigits);
            var maxNumber = (int)Math.Pow(10, _maxDigits);

            while (true) {
                var refnumber = Prefix + string.Format(format, start);
                var exists = CustomerQueries.IsCustomerExistsByRefNumber(refnumber);
                if (exists.HasValue) {
                    if (!exists.Value)
                        return refnumber;

                    start = (start + 1) % maxNumber;
                }
            }
        }

        protected virtual int GenerateRandomNumber(int digits) {
            lock (_rnd) {
                return _rnd.Next(1, (int)Math.Pow(10, digits));
            }
        }
    }
}
