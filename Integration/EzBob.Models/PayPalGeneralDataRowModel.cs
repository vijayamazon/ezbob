using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PayPalGeneralDataRowModel
    {
        private bool _pounds = true;

        public PayPalGeneralDataRowModel()
        {
            IsShares = false;
            Type = "-";
            M1 = 0.0;
            M3 = 0.0;
            M6 = 0.0;
            M12 = 0.0;
            M15 = 0.0;
            M18 = 0.0;
            M24 = 0.0;
            M24Plus = 0.0;
        }

        public PayPalGeneralDataRowModel(PayPalGeneralDetailDataRow row)
        {
            IsShares = false;
            Type = row.Type;
            M1 = row.M1;
            M3 = row.M3;
            M6 = row.M6;
            M12 = row.M12;
            M15 = row.M15;
            M18 = row.M18;
            M24 = row.M24;
            M24Plus = row.M24Plus;
        }

        public bool IsShares { get; set; }
        public string Type { get; set; }
        public double? Percent { get; set; }

        public double? M1 { get; set; }
        public double? M3 { get; set; }
        public double? M6 { get; set; }
        public double? M12 { get; set; }
        public double? M15 { get; set; }
        public double? M18 { get; set; }
        public double? M24 { get; set; }
        public double? M24Plus { get; set; }

        public IEnumerable<double?> Values()
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
        public double? FirstNotNull()
        {
            return Values().LastOrDefault(v => v != null);
        }

        public bool Pounds
        {
            get { return _pounds; }
            set { _pounds = value; }
        }
    }
}