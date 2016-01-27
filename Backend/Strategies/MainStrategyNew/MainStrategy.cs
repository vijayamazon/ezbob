namespace Ezbob.Backend.Strategies.MainStrategyNew {
	using Ezbob.Backend.Strategies.Exceptions;

	public class MainStrategy : AStrategy {
		public MainStrategy(MainStrategyArguments args) {
			if (args == null)
				throw new StrategyAlert(this, "No arguments specified for the main strategy.");

			this.context = new MainStrategyContextData(args);
		} // constructor

		public override string Name {
			get { return "Main strategy"; }
		} // Name

		public override void Execute() {
		} // Execute

		private readonly MainStrategyContextData context;
	} // class MainStrategy
} // namespace

