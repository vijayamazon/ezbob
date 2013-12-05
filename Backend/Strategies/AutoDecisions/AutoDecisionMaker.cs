namespace Strategies.AutoDecisions
{
	using EzBob.Backend.Strategies;
	using log4net;

	public class AutoDecisionMaker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AutoDecisionMaker));

		// TODO: move all configs \ db values that are used here but not outside here
		public static void MakeDecision(MainStrategy mainStrategy)
		{
			if (new ReRejection().MakeDecision(mainStrategy))
			{
				return;
			}
			
			if (new ReApproval().MakeDecision(mainStrategy))
			{
				return;
			}

			if (new Approval().MakeDecision(mainStrategy))
			{
				return;
			}

			new Rejection().MakeDecision(mainStrategy);
		}
	}
}
