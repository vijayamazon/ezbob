namespace EzBob.Backend.Strategies.AutoDecisions
{
	using log4net;

	public class AutoDecisionMaker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AutoDecisionMaker));

		public static AutoDecisionResponse MakeDecision(AutoDecisionRequest request)
		{
			var autoDecisionResponse = new AutoDecisionResponse(request);

			if (new ReRejection(request).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new ReApproval(request).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Approval(request).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Rejection(request).MakeDecision(autoDecisionResponse))
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
