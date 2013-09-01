using System;
using System.Collections.Generic;

namespace Ezbob.HmrcHarvester {
	#region class RtiTaxYearSeeds

	public class RtiTaxYearSeeds : ISeeds {
		#region public

		#region constructor

		public RtiTaxYearSeeds() {
			Months = new List<RtiTaxMonthSeed>();
		} // constructor

		#endregion constructor

		#region property Months

		public List<RtiTaxMonthSeed> Months { get; private set; }

		#endregion property Months

		#endregion public
	} // class RtiTaxYearSeeds

	#endregion class RtiTaxYearSeeds
} // namespace Ezbob.HmrcHarvester
