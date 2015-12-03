namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using System;
	using System.Globalization;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class TestHarvester : IHarvester {
		public TestHarvester(ASafeLog log) {
			this.log = log.Safe();
			ReplyMode = ReplyModes.Random;
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration cfg) {
			string reply = GetReply();

			this.log.Debug("Chosen reply in test harvester:\n{0}", reply);

			return new Response<Reply>(reply);
		} // Infer

		public ReplyModes ReplyMode { get; set; }

		public enum ReplyModes {
			Random,
			BadInferenceRequest,
			BadEtlRequest,
			EquifaxTimeout,
			LogicalGlueInferenceApiTimeout,
			HardRejection,
		} // enum ReplyModes

		private string GetReply() {
			string now = DateTime.UtcNow.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			return ChooseReply().Replace("__NOW__", now);
		} // GetReply

		private string ChooseReply() {
			switch (ReplyMode) {
			case ReplyModes.BadInferenceRequest:
				return BadInferenceRequest;

			case ReplyModes.BadEtlRequest:
				return BadEtlRequest;

			case ReplyModes.EquifaxTimeout:
				return EquifaxTimout;

			case ReplyModes.LogicalGlueInferenceApiTimeout:
				return LogicalGlueInferenceApiTimout;

			case ReplyModes.HardRejection:
				return HardRejection;
			} // switch

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
		""message"": ""Successfully processed '__NOW__'""
	},
	""equifax"": ""<xml>Equifax data for bad inference request '__NOW__'.</xml>""
}";

		private const string BadEtlRequest = @"{
	""status"": 400,
	""error"": ""Company Identifier must be a number '__NOW__'""
}";

		private const string EquifaxTimout = @"{
	""status"": 504,
	""timeout"": ""E"",
	""error"": ""Equifax Unavailable '__NOW__'""
}";

		private const string LogicalGlueInferenceApiTimout = @"{
	""status"": 504,
	""timeout"": ""L"",
	""error"": ""Logical Glue Inference Service Unavailable '__NOW__'"",
	""equifax"": ""<xml>Equifax data for LG inference API timeout '__NOW__'.</xml>""
}";

		private const string HardRejection = @"{
	""status"": 200,
	""etl"": {
		""code"": ""R"",
		""message"": ""Hard Rejection Rule Fired: Company is less than 2 years old '__NOW__'""
	},
	""equifax"": ""<xml>Equifax data for hard rejection '__NOW__'.</xml>""
}";

		private readonly ASafeLog log;
	} // class TestHarvester
} // namespace
