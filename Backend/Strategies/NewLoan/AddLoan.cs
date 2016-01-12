namespace Ezbob.Backend.Strategies.NewLoan {
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	public class AddLoan : AStrategy {

		public AddLoan(NL_Model nlModel) {
			NLModel = nlModel;
			Loader = new NL_Loader(NLModel);

		
		}//constructor

		public override string Name { get { return "AddLoan"; } }

		public override void Execute() {
		}//Execute


		
		public int LoanID;
		public NL_Loader Loader { get; private set; }
		public NL_Model NLModel { get; private set; }

	
	}//class AddLoan
}//ns