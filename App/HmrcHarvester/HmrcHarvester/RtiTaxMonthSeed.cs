using System;

namespace Ezbob.HmrcHarvester {

	public class RtiTaxMonthSeed {

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }

		public Coin AmountPaid { get; set; }
		public Coin AmountDue { get; set; }

	} // class RtiTaxMonthSeed

} // namespace Ezbob.HmrcHarvester
