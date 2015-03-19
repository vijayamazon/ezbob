namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	public class ParseSale : AStrategy {
	public override string Name {
			get { return "Alibaba ParseSale"; }
		}

		public ParseSale(byte[] doc) {
			this.Document = doc;
	//		Result = new BoolActionResult();
		}

		public override void Execute() {

			// check the document exists is not empty
			if (this.Document == null || this.Document.Length < 2) {
				Message = "Bad document attached";
	//			Result.Value = false;
				return;
			}

			// parse doc


			// extract AlibabaBuyer from each set of sale data

			// save to [dbo].[AlibabaSale] and related in [dbo].[AlibabaBuyer]

			// return array  with results
		}




		public byte[] Document;

	//	public BoolActionResult Result { get; private set; }

		public string Message { get; private set; }
	}

}