using System;
using System.Collections.Generic;

namespace Ezbob.HmrcHarvester {

	public class RtiTaxYearSeeds : ISeeds {

		public RtiTaxYearSeeds() {
			Months = new List<RtiTaxMonthSeed>();
		} // constructor

		public List<RtiTaxMonthSeed> Months { get; private set; }

	} // class RtiTaxYearSeeds

} // namespace Ezbob.HmrcHarvester
