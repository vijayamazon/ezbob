namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetAffordabilityModel : AStrategy
	{
		public GetAffordabilityModel(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "GetAffordabilityModel"; }
		}

		public List<AffordabilityModel> AffordabilityModels { get; private set; }

		public override void Execute()
		{
			
		}
	}
}
