namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;

	public class AutoDecisionMaker
	{
		public static AutoDecisionResponse MakeDecision(AutoDecisionRequest request, AConnection oDb)
		{
			var autoDecisionResponse = new AutoDecisionResponse(request);

			if (new ReRejection(request, oDb).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new ReApproval(request, oDb).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Approval(request, oDb).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Rejection(request, oDb).MakeDecision(autoDecisionResponse))
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
