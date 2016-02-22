namespace Ezbob.Backend.Strategies.LogicalGlue {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	
	public class GetHistoryInferences : AStrategy {
		/// <param name="customerID">ID of customer to infer.</param>
		public GetHistoryInferences(int customerID) {
			this.lgEngine = InjectorStub.GetEngine(); // This call should some day be replaced with real injection.
			this.now = DateTime.UtcNow;
			this.customerID = customerID;
			Inferences = new List<Inference>();
		} // constructor

		public override string Name {
			get { return "GetInference"; }
		} // Name

		public override void Execute() {
			Inferences = this.lgEngine.GetInferenceHistory(this.customerID, this.now, true, null);
		} // Execute

		public IList<Inference> Inferences { get; private set; }

		private readonly IEngine lgEngine;
		private readonly int customerID;
		private DateTime now;
	} // class GetHistoryInferences
} // namespace

