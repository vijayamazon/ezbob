namespace Ezbob.Backend.Strategies {
	using System;

	public class LoanModel {
		public DateTime SomeTime { get; set; }
	} // class LoanModel

	public class Loader {
		public Loader(LoanModel model) {
			this.model = model;
		} // constructor

		public void ExampleMethod(int customerID) {
			model.SomeTime = DateTime.UtcNow.AddSeconds(customerID);
		} // ExampleMethod

		private readonly LoanModel model;
	} // class Loader

	public class LoaderStrategy : AStrategy {
		public LoaderStrategy(Action<Loader> action) {
			Model = new LoanModel();
			this.loader = new Loader(Model);
			this.action = action;
		} // constructor

		public override string Name {
			get { return "LoaderPrototype"; }
		} // Name

		public override void Execute() {
			if (this.action != null)
				this.action(this.loader);
		} // Execute

		public LoanModel Model { get; private set; }

		private readonly Action<Loader> action;
		private readonly Loader loader;
	} // class LoaderStrategy
} // namespace

