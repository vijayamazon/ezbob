namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using Ezbob.Integration.LogicalGlue.Exceptions.Harvester;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class Harvester : IHarvester {
		public Harvester(ASafeLog log) {
			this.log = log.Safe();
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration cfg) {
			ValidateInput(inputData, cfg);

			using (var restClient = new HttpClient { BaseAddress = new Uri("https://" + cfg.HostName), }) {
				
			} // using

			return new Response<Reply>(""); // TODO
		} // Infer

		private void ValidateInput(InferenceInput inputData, HarvesterConfiguration cfg) {
			if (cfg == null)
				throw new NoConfigurationAlert(this.log);

			List<string> errors = cfg.Validate();

			if (errors != null)
				throw new BadConfigurationAlert(errors, this.log);

			if (inputData == null)
				throw new NoInputDataAlert(this.log);

			errors = inputData.Validate();

			if (errors != null)
				throw new BadInputDataAlert(errors, this.log);
		} // ValidateInput

		private readonly ASafeLog log;
	} // class Harvester
} // namespace
