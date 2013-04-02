using System.Collections.Generic;
using System.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
    public class PayPalGeneralDetailDataRow
    {
        public virtual string Type { get; set; }
        public virtual double? M1 { get; set; }
        public virtual double? M3 { get; set; }
        public virtual double? M6 { get; set; }
        public virtual double? M12 { get; set; }
        public virtual double? M15 { get; set; }
        public virtual double? M18 { get; set; }
        public virtual double? M24 { get; set; }
        public virtual double? M24Plus { get; set; }


        public virtual IEnumerable<double?> Values()
        {
            yield return M1;
            yield return M3;
            yield return M6;
            yield return M12;
            yield return M15;
            yield return M18;
            yield return M24;
            yield return M24Plus;
        }

        /// <summary>
        /// return first not null value, but if all values are null, returns null too
        /// </summary>
        /// <returns></returns>
        public virtual double? FirstNotNull()
        {
            return Values().LastOrDefault(v => v != null);
        }
    }
}
