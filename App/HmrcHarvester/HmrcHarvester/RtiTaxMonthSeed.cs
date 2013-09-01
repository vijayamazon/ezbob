using System;

namespace Ezbob.HmrcHarvester {
	#region class RtiTaxMonthSeed

	public class RtiTaxMonthSeed {
		#region public

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }

		public Coin AmountPaid { get; set; }
		public Coin AmountDue { get; set; }

		#endregion public
	} // class RtiTaxMonthSeed

	#endregion class RtiTaxMonthSeed
} // namespace Ezbob.HmrcHarvester
