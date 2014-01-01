namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoDecisionMaker
	{
		public static AutoDecisionResponse MakeDecision(AutoDecisionRequest request, AConnection oDb, ASafeLog oLog)
		{
			var autoDecisionResponse = new AutoDecisionResponse(request);

			if (new ReRejection(request, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new ReApproval(request, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Approval(request, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Rejection(request, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}


			autoDecisionResponse.CreditResult = "WaitingForDecision";
			autoDecisionResponse.UserStatus = "Manual";
			autoDecisionResponse.SystemDecision = "Manual";

			return autoDecisionResponse;
		}
	}
}
