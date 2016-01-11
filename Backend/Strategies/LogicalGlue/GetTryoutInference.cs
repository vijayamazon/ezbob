namespace Ezbob.Backend.Strategies.LogicalGlue {
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	
	public class GetTryoutInference : AStrategy {
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="monthlyRepayment"></param>
		/// <param name="isTryOut"></param>
		public GetTryoutInference(int customerID, decimal monthlyRepayment, bool isTryOut) {
			this.lgEngine = InjectorStub.GetEngine(); // This call should some day be replaced with real injection.

			this.customerID = customerID;
			this.monthlyRepayment = monthlyRepayment;
			this.isTryOut = isTryOut;

			Inference = null;
		} // constructor

		public override string Name {
			get { return "GetInference"; }
		} // Name

		public override void Execute() {
			Inference = this.lgEngine.GetInference(this.customerID, this.monthlyRepayment, this.isTryOut, GetInferenceMode.ForceDownload);
		} // Execute

		public Inference Inference { get; private set; }

		private readonly IEngine lgEngine;
		private readonly int customerID;
		private readonly decimal monthlyRepayment;
		private readonly bool isTryOut;
	} // class GetTryoutInference
} // namespace

