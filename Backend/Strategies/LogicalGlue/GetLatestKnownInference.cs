namespace Ezbob.Backend.Strategies.LogicalGlue {
	using System;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;

	/// <summary>
	/// Loads the latest customer inference results that were available on specific time.
	/// Logical Glue API is not queried.
	/// 
	/// The latest customer inference results that were available on requested time (can be NULL)
	/// are loaded into <see cref="Inference"/> property.
	/// </summary>
	public class GetLatestKnownInference : AStrategy {
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="forTime">Time of interest.</param>
		/// <param name="includeTryOuts">Include try out data or not.</param>
		public GetLatestKnownInference(int customerID, DateTime? forTime, bool includeTryOuts) {
			this.lgEngine = InjectorStub.GetEngine(); // This call should some day be replaced with real injection.

			this.customerID = customerID;
			this.forTime = forTime ?? DateTime.UtcNow;
			this.includeTryOuts = includeTryOuts;

			Inference = null;
		} // constructor

		public override string Name {
			get { return "GetInference"; }
		} // Name

		public override void Execute() {
			Inference = this.lgEngine.GetInference(this.customerID, this.forTime, this.includeTryOuts, 0);
		} // Execute

		public Inference Inference { get; private set; }

		private readonly IEngine lgEngine;
		private readonly int customerID;
		private readonly DateTime forTime;
		private readonly bool includeTryOuts;
	} // class GetLatestKnownInference
} // namespace

