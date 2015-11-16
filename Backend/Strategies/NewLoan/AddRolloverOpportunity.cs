namespace Ezbob.Backend.Strategies.NewLoan {
	public class AddRolloverOpportunity : AStrategy {

		public AddRolloverOpportunity() {

		
		}

		public override string Name { get { return "AddRolloverOpportunity"; } }
		
		public string Error;
		private readonly object[] strategyArgs;

		public override void Execute() {

			// TODO EZ-4329 records opportunity for rollover (insert into NL_LoanRollovers) + record into NL_LoanFees with feetype of RolloverFee

		}

	} // class AddRolloverOpportunity
} // ns