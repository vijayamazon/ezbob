namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.Models.NewLoan;

	public class AddPayment : AStrategy {

		public AddPayment(NL_Model nlModel) {

			NLModel = nlModel;
			Loader = new NL_Loader(NLModel);

		} // constructor

		public override string Name { get { return "AddPayment"; } }

	
		public override void Execute() {

		

		} // Execute




		public NL_Loader Loader { get; private set; }
		public NL_Model NLModel { get; private set; }


	} // class AddPayment
} // ns