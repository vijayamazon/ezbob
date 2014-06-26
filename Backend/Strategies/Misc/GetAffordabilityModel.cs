namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class GetAffordabilityModel : AStrategy
	{
		private readonly SpGetPayPalAffordabilityModel sp;

		public GetAffordabilityModel(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			AffordabilityModels = new List<AffordabilityModel>();
			sp = new SpGetPayPalAffordabilityModel(DB, Log)
			{
				CustomerId = customerId
			};
		}

		public override string Name {
			get { return "GetAffordabilityModel"; }
		}

		public List<AffordabilityModel> AffordabilityModels { get; private set; }

		public class SimpleAffordabilityResult : ITraversable
		{
			public DateTime DateFrom { get; set; }
			public DateTime DateTo { get; set; }
			public decimal Revenues { get; set; }
		}

		private class SpGetPayPalAffordabilityModel : AStoredProc
		{
			public SpGetPayPalAffordabilityModel(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { }

			public override bool HasValidParameters()
			{
				return true;
			}

			public int CustomerId { get; set; }
		}

		public override void Execute()
		{
			AffordabilityModel payPalModel = new AffordabilityModel();
			payPalModel.Type = AffordabilityType.PSP;

			SimpleAffordabilityResult payPalSimpleAffordabilityResult = sp.FillFirst<SimpleAffordabilityResult>();

			payPalModel.Revenues = payPalSimpleAffordabilityResult.Revenues;
			payPalModel.DateFrom = payPalSimpleAffordabilityResult.DateFrom;
			payPalModel.DateTo = payPalSimpleAffordabilityResult.DateTo;
			payPalModel.HasFullYearData = payPalModel.DateFrom.AddYears(1) <= payPalModel.DateTo;

			AffordabilityModels.Add(payPalModel);
		}
	}
}
