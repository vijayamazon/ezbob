namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using System;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class TestHarvester : IHarvester {
		public TestHarvester(ASafeLog log) {
			this.log = log.Safe();
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration cfg) {
			string reply = ChooseReply();

			this.log.Debug("Chosen reply in test harvester:\n{0}", reply);

			return new Response<Reply>(reply);
		} // Infer

		private string ChooseReply() {
			int rnd = new Random().Next(1, 101);

			if (rnd <= 20)
				return BadInferenceRequest;

			if (rnd <= 40)
				return BadEtlRequest;

			if (rnd <= 60)
				return EquifaxTimout;

			if (rnd <= 80)
				return LogicalGlueInferenceApiTimout;
			
			return HardRejection;
		} // ChooseReply

		private const string BadInferenceRequest = @"{
	""status"": 400,
	""etl"": {
		""code"": ""P"",
		""message"": ""Successfully processed""
	},
	""equifax"": ""<xml>Equifax data for bad inference request.</xml>""
}";

		private const string BadEtlRequest = @"{
	""status"": 400,
	""error"": ""Company Identifier must be a number""
}";

		private const string EquifaxTimout = @"{
	""status"": 504,
	""timeout"": ""E"",
	""error"": ""Equifax Unavailable""
}";

		private const string LogicalGlueInferenceApiTimout = @"{
	""status"": 504,
	""timeout"": ""L"",
	""error"": ""Logical Glue Inference Service Unavailable"",
	""equifax"": ""<xml>Equifax data for LG inference API timeout.</xml>""
}";

		private const string HardRejection = @"{
	""status"": 200,
	""etl"": {
		""code"": ""R"",
		""message"": ""Hard Rejection Rule Fired: Company is less than 2 years old""
	},
	""equifax"": ""<xml>Equifax data for hard rejection.</xml>""
}";

		private readonly ASafeLog log;
	} // class TestHarvester
} // namespace
