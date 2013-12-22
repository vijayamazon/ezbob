using Ezbob.Database;

namespace EzBob.Backend.Strategies.AutoDecisions
{
	public class AutoDecisionMaker
	{
		public static AutoDecisionResponse MakeDecision(AutoDecisionRequest request, AConnection oDB)
		{
			var autoDecisionResponse = new AutoDecisionResponse(request);

			if (new ReRejection(request, oDB).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new ReApproval(request, oDB).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Approval(request, oDB).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Rejection(request, oDB).MakeDecision(autoDecisionResponse))
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
